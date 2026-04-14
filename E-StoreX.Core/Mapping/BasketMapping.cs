using AutoMapper;
using Domain.Entities.Baskets;
using EStoreX.Core.DTO.Basket;

namespace EStoreX.Core.Mapping
{
    public class BasketMapping : Profile
    {
        public BasketMapping()
        {
            CreateMap<CustomerBasket, CustomerBasketDTO>()
                .ForMember(dest => dest.BasketItems,
                    opt => opt.MapFrom(src => src.BasketItems));

            CreateMap<BasketItem, BasketItemResponse>()
                .ForMember(dest => dest.Name,
                    opt => opt.MapFrom(src =>
                        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar"
                            ? src.NameAr ?? src.NameEn
                            : src.NameEn))
                .ForMember(dest => dest.Description,
                    opt => opt.MapFrom(src =>
                        System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar"
                            ? src.DescriptionAr ?? src.DescriptionEn
                            : src.DescriptionEn));
        }
    }
}
