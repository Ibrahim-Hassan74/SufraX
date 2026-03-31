using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Brands.Response;

namespace EStoreX.Core.Mapping
{
    public class BrandMapping : Profile
    {
        public BrandMapping()
        {
            CreateMap<Brand, BrandResponse>()
                .ForMember(dest => dest.Photos, opt => opt.MapFrom(src => src.Photos))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.NameAr ?? src.NameEn : src.NameEn));
        }
    }
}
