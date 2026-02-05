using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Products.Responses;
using EStoreX.Core.Enums;
using EStoreX.Core.RepositoryContracts.Products;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Infrastructure.Data;
using EStoreX.Infrastructure.Repository.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;

namespace Repository.Products
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IPhotoRepository _photosRepository;
        private readonly IImageService _imageService;
        public ProductRepository(ApplicationDbContext context, IPhotoRepository photosRepository, IImageService imageService) : base(context)
        {
            _context = context;
            _photosRepository = photosRepository;
            _imageService = imageService;
        }

        public async Task<Product> AddProductAsync(Product product, IFormFileCollection formFiles)
        {

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            var photosDTO = new PhotosDTO()
            {
                ProductId = product.Id,
                Src = product.Name,
                FormFiles = formFiles
            };
            var photos = await _photosRepository.AddRangeAsync(photosDTO);

            #region Commented out code for image handling
            //var imagePath = await _imageService.AddImageAsync(productRequest.Photos, productRequest.Name);

            //var photos = imagePath.Select(path => new Photo
            //{
            //    ImageName = path,
            //    ProductId = product.Id,
            //    Id = Guid.NewGuid()
            //}).ToList();

            //await _context.AddRangeAsync(photos);
            //await _context.SaveChangesAsync();
            #endregion

            return product;
        }

        public async Task<bool> DeleteAsync(Product product)
        {
            var photos = await _context.Photos.Where(x => x.ProductId == product.Id).ToListAsync();

            foreach (var photo in photos)
            {
                if (!string.IsNullOrEmpty(photo.ImageName))
                    _imageService.DeleteImageAsync(photo.ImageName);
            }

            _context.Products.Remove(product);
            var res = await _context.SaveChangesAsync();
            return res > 0;
        }

        public async Task<Product> UpdateProductAsync(Product product, IFormFileCollection formFiles)
        {
            var res = await UpdateAsync(product);
            await _context.SaveChangesAsync();

            PhotosDTO photosDTO = new PhotosDTO()
            {
                ProductId = product.Id,
                Src = product.Name,
                FormFiles = formFiles,
                Photos = product.Photos
            };

            if (photosDTO is not null)
            {
                res.Photos = (await _photosRepository.UpdatePhotosAsync(photosDTO)).ToList();
            }


            return res;
        }

        public async Task<(IEnumerable<Product>, int)> GetFilteredProductsAsync(ProductQueryDTO query)
        {
            IQueryable<Product> products = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Photos)
                .Include(p => p.Brand);

            products = ApplyFiltering(products, query);
            products = ApplySorting(products, query);
            var size = products.Count();
            products = ApplyPagination(products, query);

            return (await products.ToListAsync(), size);
        }


        private IQueryable<Product> ApplyFiltering(IQueryable<Product> products, ProductQueryDTO query)
        {
            if (!string.IsNullOrWhiteSpace(query.SearchString))
            {
                switch (query.SearchBy)
                {
                    case SearchByOptions.Name:
                        products = products.Where(p =>
                            EF.Functions.FreeText(p.Name, query.SearchString) ||
                            ApplicationDbContext.Soundex(p.Name) == ApplicationDbContext.Soundex(query.SearchString));
                        break;

                    case SearchByOptions.Description:
                        products = products.Where(p =>
                            EF.Functions.FreeText(p.Description, query.SearchString) ||
                            ApplicationDbContext.Soundex(p.Description) == ApplicationDbContext.Soundex(query.SearchString));
                        break;

                    case SearchByOptions.Category:
                        products = products.Where(p => p.Category.Name.Contains(query.SearchString));
                        break;

                    case SearchByOptions.Brand:
                        products = products.Where(p => p.Brand.Name.Contains(query.SearchString));
                        break;
                    case SearchByOptions.None:
                    default:
                        products = products.Where(p => EF.Functions.FreeText(p.Name, query.SearchString) ||
                        EF.Functions.FreeText(p.Description, query.SearchString) ||
                        ApplicationDbContext.Soundex(p.Name) == ApplicationDbContext.Soundex(query.SearchString) ||
                        ApplicationDbContext.Soundex(p.Description) == ApplicationDbContext.Soundex(query.SearchString));
                        break;
                }
            }

            if (query.CategoryId.HasValue)
            {
                products = products.Where(p => p.CategoryId == query.CategoryId.Value);
            }

            if (query.MinPrice.HasValue)
            {
                products = products.Where(p => p.NewPrice >= query.MinPrice.Value);
            }

            if (query.MaxPrice.HasValue)
            {
                products = products.Where(p => p.NewPrice <= query.MaxPrice.Value);
            }

            if (query.BrandId.HasValue)
            {
                products = products.Where(p => p.BrandId == query.BrandId.Value);
            }

            return products;
        }

        private IQueryable<Product> ApplySorting(IQueryable<Product> products, ProductQueryDTO query)
        {
            //var sortBy = query.SortBy ?? nameof(Product.NewPrice);
            bool isAscending = query.SortOrder == SortOrderOptions.ASC;

            //switch (sortBy)
            //{
            //    case var s when s == nameof(Product.Name):
            //        products = isAscending
            //            ? products.OrderBy(p => p.Name)
            //            : products.OrderByDescending(p => p.Name);
            //        break;

            //    case var s when s == nameof(Product.NewPrice) || s == "Price":
            //        products = isAscending
            //            ? products.OrderBy(p => p.NewPrice)
            //            : products.OrderByDescending(p => p.NewPrice);
            //        break;

            //    case var s when s == nameof(Product.OldPrice):
            //        products = isAscending
            //            ? products.OrderBy(p => p.OldPrice)
            //            : products.OrderByDescending(p => p.OldPrice);
            //        break;

            //    case "Category":
            //        products = isAscending
            //            ? products.OrderBy(p => p.Category.Name)
            //            : products.OrderByDescending(p => p.Category.Name);
            //        break;

            //    case nameof(Product.SalesCount):
            //        products = isAscending
            //            ? products.OrderBy(p => p.SalesCount)
            //            : products.OrderByDescending(p => p.SalesCount);
            //        break;

            //    default:
            //        products = isAscending
            //            ? products.OrderBy(p => p.NewPrice)
            //            : products.OrderByDescending(p => p.NewPrice);
            //        break;
            //}

            switch (query.SortBy ?? ProductSortBy.Price)
            {
                case ProductSortBy.Name:
                    products = isAscending
                        ? products.OrderBy(p => p.Name)
                        : products.OrderByDescending(p => p.Name);
                    break;

                case ProductSortBy.Price:
                    products = isAscending
                        ? products.OrderBy(p => p.NewPrice)
                        : products.OrderByDescending(p => p.NewPrice);
                    break;
                case ProductSortBy.OldPrice:
                    products = isAscending
                    ? products.OrderBy(p => p.OldPrice)
                    : products.OrderByDescending(p => p.OldPrice);
                    break;

                case ProductSortBy.Category:
                    products = isAscending
                        ? products.OrderBy(p => p.Category.Name)
                        : products.OrderByDescending(p => p.Category.Name);
                    break;

                case ProductSortBy.SalesCount:
                    products = isAscending
                        ? products.OrderBy(p => p.SalesCount)
                        : products.OrderByDescending(p => p.SalesCount);
                    break;
                default:
                    products = isAscending
                        ? products.OrderBy(p => p.NewPrice)
                        : products.OrderByDescending(p => p.NewPrice);
                    break;
            }

            return products;
        }

        private IQueryable<Product> ApplyPagination(IQueryable<Product> products, ProductQueryDTO query)
        {
            int pageNumber = query.PageNumber < 1 ? 1 : query.PageNumber;
            int pageSize = query.PageSize < 1 ? 10 : query.PageSize;

            int skipAmount = (pageNumber - 1) * pageSize;

            return products.Skip(skipAmount).Take(pageSize);
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync()
        {
            return await _context.Products
                .Include(p => p.Photos)
                .Include(p => p.Category)
                .Include(p => p.Brand)
                .Where(p => p.IsFeatured)
                .ToListAsync();
        }

        public async Task<bool> SetFeaturedStatusAsync(Guid productId, bool isFeatured)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null) return false;

            product.IsFeatured = isFeatured;
            _context.Products.Update(product);
            return await _context.SaveChangesAsync() > 0;
        }

        // You can add product-specific methods here if needed
        // For example, methods to get products by category, price range, etc.
    }
}
