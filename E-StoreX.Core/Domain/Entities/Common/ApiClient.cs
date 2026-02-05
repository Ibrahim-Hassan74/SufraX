using System.ComponentModel.DataAnnotations;

namespace Domain.Entities.Common
{
    public class ApiClient : BaseEntity<Guid>
    {
        public string? ClientName { get; set; }
        [Required(ErrorMessage = "API Key is required.")]
        public string ApiKey { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime UpdatedAt { get; set; }
        public string? OAuthCallbackUrl { get; set; }
        public string? AccountActivationCallbackUrl { get; set; }
        public string? PasswordResetCallbackUrl { get; set; }
    }
}
