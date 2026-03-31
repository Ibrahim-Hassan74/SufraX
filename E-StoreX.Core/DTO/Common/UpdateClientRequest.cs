namespace EStoreX.Core.DTO.Common
{
    public class UpdateClientRequest
    {
        public string? ClientName { get; set; }
        public string? OAuthCallbackUrl { get; set; }
        public string? AccountActivationCallbackUrl { get; set; }
        public string? PasswordResetCallbackUrl { get; set; }
        public string? Email { get; set; }
    }
}
