using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Discount.Request;
using EStoreX.Core.DTO.Discounts.Responses;

namespace EStoreX.Core.Mapping
{
    public class DiscountMapping : Profile
    {
        public DiscountMapping()
        {
            CreateMap<DiscountRequest, Discount>().ReverseMap();

            CreateMap<Discount, DiscountResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.NameEn : null))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category != null ? src.Category.NameEn : null))
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand != null ? src.Brand.NameEn : null));
        }
    }
}
