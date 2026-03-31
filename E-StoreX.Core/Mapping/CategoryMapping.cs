using AutoMapper;
using Domain.Entities.Product;
using EStoreX.Core.DTO.Categories.Requests;
using EStoreX.Core.DTO.Categories.Responses;

namespace EStoreX.Core.Mapping
{
    public class CategoryMapping : Profile
    {
        public CategoryMapping()
        {
            CreateMap<CategoryRequest, Category>();
            CreateMap<Category, CategoryResponse>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.NameAr ?? src.NameEn : src.NameEn))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.DescriptionAr ?? src.DescriptionEn : src.DescriptionEn));

            CreateMap<UpdateCategoryDTO, Category>();
            CreateMap<Category, CategoryResponseWithPhotos>()
                .ForMember(dest => dest.Photos,
                    opt => opt.MapFrom(src =>
                        src.Photos != null ? src.Photos : new List<Photo>()
                    ))
                 .ForMember(dest => dest.Name, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.NameAr ?? src.NameEn : src.NameEn))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ar" ? src.DescriptionAr ?? src.DescriptionEn : src.DescriptionEn));


        }
    }
}
