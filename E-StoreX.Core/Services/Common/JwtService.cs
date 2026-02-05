using EStoreX.Core.Domain.IdentityEntities;
using EStoreX.Core.DTO.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using EStoreX.Core.ServiceContracts.Common;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EStoreX.Core.Services.Common
{
    /// <inheritdoc/>
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        /// <inheritdoc/>
        public async Task<ApiResponse> CreateJwtToken(ApplicationUser user, bool rememberMe)
        {
            // Create a DateTime object representing the token expiration time by adding the number of minutes specified in the configuration to the current UTC time.
            DateTime expiration = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:EXPIRATION_MINUTES"]));
            var refreshTokenExpiryMinutes = rememberMe
                    ? Convert.ToDouble(_configuration["RefreshToken:LONG_EXPIRATION_MINUTES"])
                    : Convert.ToDouble(_configuration["RefreshToken:EXPIRATION_MINUTES"]);   

            // Create an array of Claim objects representing the user's claims, such as their ID, name, email, etc.
            List<Claim> claims = new List<Claim>
            {
                 new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), //Subject (user id)
                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), //JWT unique ID
                 new Claim(JwtRegisteredClaimNames.Iat,
                    new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64), //Issued at (date and time of token generation)
                 new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), //Unique name identifier of the user Id
                 new Claim(ClaimTypes.Email, user.Email), //Email of the user
                 new Claim("remember_me", rememberMe.ToString().ToLower())
             };

            var audienceClaims = _configuration.GetSection("Jwt:Audiences").Get<string[]>();
            if (audienceClaims is not null)
            {
                foreach (var aud in audienceClaims)
                {
                    claims.Add(new Claim(JwtRegisteredClaimNames.Aud, aud));
                }
            }

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Create a SymmetricSecurityKey object using the key specified in the configuration.
            SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

            // Create a SigningCredentials object with the security key and the HMACSHA256 algorithm.
            SigningCredentials signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Create a JwtSecurityToken object with the given issuer, audience, claims, expiration, and signing credentials.
            JwtSecurityToken tokenGenerator = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: expiration,
                signingCredentials: signingCredentials
            );

            // Create a JwtSecurityTokenHandler object and use it to write the token as a string.
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            string token = tokenHandler.WriteToken(tokenGenerator);

            var refreshTokenExpiry = DateTimeOffset.UtcNow.AddMinutes(refreshTokenExpiryMinutes);

            // Create and return an AuthenticationResponse object containing the token, user email, user name, and token expiration time.
            return new ApiSuccessResponse()
            {
                Token = token,
                Email = user.Email,
                UserName = user.DisplayName,
                Expiration = expiration,
                RefreshToken = GenerateRefreshToken(),
                RefreshTokenExpirationDateTime = refreshTokenExpiry,
            };
        }

        /// <summary>
        /// Create Refresh Token (base 64 string of random number)
        /// </summary>
        /// <returns>Refresh Token</returns>
        private string GenerateRefreshToken()
        {
            byte[] bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }
        /// <inheritdoc/>
        public ClaimsPrincipal? GetPrincipalFromJwtToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = false,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudiences = _configuration.GetSection("Jwt:Audiences").Get<List<string>>(),
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                RoleClaimType = ClaimTypes.Role
            };

            JwtSecurityTokenHandler jwtSecurityTokenHandler = new JwtSecurityTokenHandler();
            ClaimsPrincipal claimsPrincipal =
                jwtSecurityTokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            return claimsPrincipal;
        }
    }
}
