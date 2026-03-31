using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using System.ComponentModel.DataAnnotations;
using EStoreX.Core.Enums;

namespace EStoreX.Core.DTO.Discount.Request
{
    public class DiscountRequest : IValidatableObject
    {
        [Range(0, 100, ErrorMessageResourceName = "InvalidPercentageRange", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Discount.DiscountValidationMessages))]
        public decimal Percentage { get; set; }

        [Required(ErrorMessageResourceName = "RequiredDiscountType", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Discount.DiscountValidationMessages))]
        public DiscountType DiscountType { get; set; }

        public Guid? ProductId { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }

        [Required(ErrorMessageResourceName = "RequiredStartDate", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Discount.DiscountValidationMessages))]
        public DateTime StartDate { get; set; } = DateTime.UtcNow;

        public DateTime? EndDate { get; set; }
        [Range(1, int.MaxValue, ErrorMessageResourceName = "InvalidMaxUsageCount", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Discount.DiscountValidationMessages))]
        public int MaxUsageCount { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var localizer = validationContext.GetService(typeof(IStringLocalizer<EStoreX.Core.Resources.DTO.Discount.DiscountValidationMessages>)) as IStringLocalizer<EStoreX.Core.Resources.DTO.Discount.DiscountValidationMessages>;
            switch (DiscountType)
            {
                case DiscountType.Product:
                    if (!ProductId.HasValue)
                        yield return new ValidationResult(localizer?["RequiredProductIdForType"].Value ?? "ProductId is required when DiscountType is Product.", new[] { nameof(ProductId) });
                    break;
                case DiscountType.Category:
                    if (!CategoryId.HasValue)
                        yield return new ValidationResult(localizer?["RequiredCategoryIdForType"].Value ?? "CategoryId is required when DiscountType is Category.", new[] { nameof(CategoryId) });
                    break;
                case DiscountType.Brand:
                    if (!BrandId.HasValue)
                        yield return new ValidationResult(localizer?["RequiredBrandIdForType"].Value ?? "BrandId is required when DiscountType is Brand.", new[] { nameof(BrandId) });
                    break;
            }
        }
    }
}
