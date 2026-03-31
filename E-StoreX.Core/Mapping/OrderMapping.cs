using AutoMapper;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.Domain.Entities.Orders;
using Domain.Entities.Common;

namespace EStoreX.Core.Mapping
{
    public class OrderMapping : Profile
    {
        public OrderMapping()
        {
            CreateMap<ShippingAddress, ShippingAddressDTO>().ReverseMap();

            CreateMap<DeliveryMethod, DeliveryMethodResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.NameAr ?? src.NameEn : src.NameEn))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.DescriptionAr ?? src.DescriptionEn : src.DescriptionEn));



            CreateMap<DeliveryMethodRequest, DeliveryMethod>();

            CreateMap<OrderItem, OrderItemResponse>()
                .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.ProductNameAr : src.ProductNameEn));

            CreateMap<Order, OrderResponse>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.GetTotal()))
                .ForMember(dest => dest.OrderItems, opt => opt.MapFrom(src => src.OrderItems))
                .ForMember(dest => dest.DeliveryMethod, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.DeliveryMethod.NameAr : src.DeliveryMethod.NameEn))
                .ForMember(dest => dest.ShippingAddress, opt => opt.MapFrom(src => src.ShippingAddress))
                .ForMember(dest => dest.PaymentIntentId, opt => opt.MapFrom(src => src.PaymentIntentId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));


            CreateMap<Address, ShippingAddressDTO>().ReverseMap();
        }
    }
}
