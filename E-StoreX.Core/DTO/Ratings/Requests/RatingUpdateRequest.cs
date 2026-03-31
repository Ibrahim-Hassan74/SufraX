using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Ratings.Requests
{
    public class RatingUpdateRequest
    {
        [Range(1, 5, ErrorMessageResourceName = "InvalidRatingRange", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Ratings.RatingValidationMessages))]
        public int Score { get; set; }

        [MaxLength(500, ErrorMessageResourceName = "MaxLengthReview", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Ratings.RatingValidationMessages))]
        public string? Comment { get; set; }
    }

}
