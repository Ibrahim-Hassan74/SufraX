using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using EStoreX.Core.ServiceContracts.Account;
using System.Threading.Tasks;
using EStoreX.Core.Helper;
using EStoreX.Core.Domain.Options;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Middleware
{
    /// <summary>
    /// Middleware to validate API key from incoming HTTP requests.
    /// </summary>
    public class ApiKeyMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string API_KEY_HEADER_NAME;
        private readonly List<string> allowedStaticPaths;
        private readonly List<string> allowedStaticExtensions;
        private readonly SecuritySettings _securitySettings;
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiKeyMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        /// <param name="options">options setting</param>
        public ApiKeyMiddleware(RequestDelegate next, IOptions<SecuritySettings> options)
        {
            _next = next;
            _securitySettings = options.Value;
            allowedStaticExtensions = _securitySettings.AllowedStaticExtensions;
            allowedStaticPaths = _securitySettings.AllowedStaticPaths;
            API_KEY_HEADER_NAME = _securitySettings.ApiKeyHeaderName;
        }

        /// <summary>
        /// Invokes the middleware to check for a valid API key.
        /// </summary>
        /// <param name="context">The current HTTP context.</param>
        /// <param name="apiClientService">api client</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public async Task InvokeAsync(HttpContext context, IApiClientService apiClientService, IStringLocalizer<SharedResource> _localizer)
        {


            var path = context.Request.Path.Value?.ToLower();

            if (allowedStaticPaths.Any(p => path.StartsWith(p)) ||
                allowedStaticExtensions.Any(ext => path.EndsWith(ext)) ||
                context.Request.Method == HttpMethods.Options ||
                context.Request.Path.StartsWithSegments("/api/v2/frontend") ||
                context.Request.Path.StartsWithSegments("/favicon.ico") ||
                path.Contains("webhook"))
            {
                await _next(context);
                return;
            }

            if (path.Contains("/api/") && path.Contains("/account/external-login"))
            {
                if (context.Request.Query.ContainsKey(API_KEY_HEADER_NAME))
                {
                    await _next(context);
                    return;
                }
            }

            if (!context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(ApiResponseFactory.Unauthorized(_localizer["ApiKeyMissing"].Value, new List<string>
                {
                    _localizer["ApiKeyRequiredDescription"].Value
                }));
                return;
            }

            if (string.IsNullOrWhiteSpace(extractedApiKey))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsJsonAsync(ApiResponseFactory.BadRequest(_localizer["ApiKeyEmpty"].Value, new List<string>
                {
                    _localizer["ApiKeyEmptyDescription"].Value
                }));
                return;
            }

            var client = await apiClientService.GetByApiKeyAsync(extractedApiKey!);

            if (client == null)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ApiResponseFactory.Forbidden(_localizer["InvalidApiKey"].Value, new List<string>
                {
                    _localizer["InvalidApiKey"].Value
                }));
                return;
            }

            if (!client.IsActive)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(ApiResponseFactory.Forbidden( _localizer["ApiKeyDeactivated"].Value, 
                new List<string>
                {
                    _localizer["ApiKeyDeactivated"].Value
                }));
                return;
            }

            await _next(context);
        }

    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ApiKeyMiddlewareExtensions
    {
        public static IApplicationBuilder UseApiKeyMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ApiKeyMiddleware>();
        }
    }
}
