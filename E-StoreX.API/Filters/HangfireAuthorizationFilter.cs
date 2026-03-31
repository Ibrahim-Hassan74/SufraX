using EStoreX.Core.Enums;
using EStoreX.Core.ServiceContracts.Common;
using Hangfire.Annotations;
using Hangfire.Dashboard;
using System.Security.Claims;

namespace EStoreX.API.Filters
{
    public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public HangfireAuthorizationFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public bool Authorize([NotNull] DashboardContext context)
        {
            using var scope = _serviceProvider.CreateScope();
            var _jwtService = scope.ServiceProvider.GetRequiredService<IJwtService>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var _jwtSecret = configuration["Jwt:Key"];
            var httpContext = context.GetHttpContext();
            var authHeader = httpContext.Request.Query["token"].FirstOrDefault();
            authHeader ??= httpContext.Request.Cookies["token"];

            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
                return false;

            var token = authHeader.Substring("Bearer ".Length).Trim();

            try
            {
                var jwtToken = _jwtService.GetPrincipalFromJwtToken(token);
                var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
                return role == nameof(UserTypeOptions.Admin) || role == nameof(UserTypeOptions.SuperAdmin);
            }
            catch
            {
                return false;
            }
        }
    }
}
