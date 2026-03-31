using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Discount.Request
{
    public class UpdateDiscountDatesRequest
    {
        /// <summary>
        /// The new start date of the discount.
        /// </summary>
        [Required(ErrorMessageResourceName = "RequiredStartDate", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Discount.DiscountValidationMessages))]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// The new end date of the discount (optional).
        /// </summary>
        public DateTime? EndDate { get; set; }
    }

}
