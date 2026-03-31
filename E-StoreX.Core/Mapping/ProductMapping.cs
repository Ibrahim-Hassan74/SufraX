using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Products.Requests;
using EStoreX.Core.DTO.Products.Responses;

namespace EStoreX.Core.Mapping
{
    public class ProductMapping : Profile
    {
        public ProductMapping()
        {
            CreateMap<ProductAddRequest, Product>()
                .ForMember(x => x.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
                .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
                .ForMember(dest => dest.DescriptionEn, opt => opt.MapFrom(src => src.DescriptionEn))
                .ForMember(dest => dest.DescriptionAr, opt => opt.MapFrom(src => src.DescriptionAr));
                
            CreateMap<ProductUpdateRequest, Product>()
                .ForMember(x => x.Photos, opt => opt.Ignore())
                .ForMember(dest => dest.NameEn, opt => opt.MapFrom(src => src.NameEn))
                .ForMember(dest => dest.NameAr, opt => opt.MapFrom(src => src.NameAr))
                .ForMember(dest => dest.DescriptionEn, opt => opt.MapFrom(src => src.DescriptionEn))
                .ForMember(dest => dest.DescriptionAr, opt => opt.MapFrom(src => src.DescriptionAr));


            CreateMap<Product, ProductResponse>()
                .ForMember(dest => dest.CategoryName,
                    opt => opt.MapFrom(src => src.Category != null ? (System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.Category.NameAr : src.Category.NameEn) : string.Empty))
                .ForMember(dest => dest.Photos,
                    opt => opt.MapFrom(src => src.Photos))
                .ForMember(dest => dest.QuantityAvailable, opt => opt.MapFrom(src => src.QuantityAvailable))
                 .ForMember(dest => dest.BrandName, 
                 opt => opt.MapFrom(src => src.Brand != null ? (System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.Brand.NameAr : src.Brand.NameEn) : string.Empty))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.NameAr ?? src.NameEn : src.NameEn))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.DescriptionAr ?? src.DescriptionEn : src.DescriptionEn));


            CreateMap<Photo, PhotoResponse>();

            //CreateMap<ProductRequest, Product>()
            //    .ForMember(dest => dest.Photos,
            //               opt => opt.MapFrom(src => src.Photos));

            CreateMap<PhotoRequest, Photo>()
                .ForMember(dest => dest.ProductId, opt => opt.Ignore());

            // enable reverse mapping if needed
            //.ReverseMap();


        }
    }
}
