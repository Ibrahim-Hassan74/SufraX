namespace EStoreX.Core.DTO.Common
{
    public class ApiSuccessResponse : ApiResponse
    {
        public string? UserName { get; set; } = string.Empty;
        public string? Email { get; set; } = string.Empty;
        public string? Token { get; set; } = string.Empty;
        //public DateTime? Expiration { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTimeOffset RefreshTokenExpirationDateTime { get; set; }
    }
}
