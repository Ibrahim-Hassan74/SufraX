using AutoMapper;
using EStoreX.Core.Domain.Entities.Rating;
using EStoreX.Core.DTO.Ratings.Requests;
using EStoreX.Core.DTO.Ratings.Response;

namespace EStoreX.Core.Mapping
{
    public class RatingProfile : Profile
    {
        public RatingProfile()
        {
            CreateMap<RatingAddRequest, Rating>();
            CreateMap<RatingUpdateRequest, Rating>();
            CreateMap<Rating, RatingResponse>()
               .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.DisplayName ?? src.User.UserName));
            CreateMap<Rating, AdminRatingResponse>()
           .ForMember(dest => dest.RatingId, opt => opt.MapFrom(src => src.Id))
           .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
           .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.NameEn))
           .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Product.Brand.NameEn))
           .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Product.Category.NameEn))
           .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.UserName))
           .ForMember(dest => dest.CommentContent, opt => opt.MapFrom(src => src.Comment))
           .ForMember(dest => dest.Score, opt => opt.MapFrom(src => src.Score));
        }
    }
}
