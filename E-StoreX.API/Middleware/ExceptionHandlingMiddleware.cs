using EStoreX.Core.Helper;
using Microsoft.Extensions.Caching.Memory;
using EStoreX.API.Filters;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Text.Json;

namespace EStoreX.API.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _host;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _rateLimiter = TimeSpan.FromSeconds(30);

        public ExceptionHandlingMiddleware(RequestDelegate next, IHostEnvironment host, IMemoryCache cache)
        {
            _next = next;
            _host = host;
            _cache = cache;
        }

        public async Task Invoke(HttpContext httpContext, IStringLocalizer<SharedResource> localizer)
        {
            try
            {
                ApplySecurity(httpContext);

                //if (!IsRequestAllowed(httpContext))
                //{
                //    httpContext.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;

                //    httpContext.Response.ContentType = "application/json";

                //    var response = new ApiExceptions((int)HttpStatusCode.TooManyRequests, "Too Many Request .. Please Try again Later");
                //    await httpContext.Response.WriteAsJsonAsync(response);
                //    return;
                //}

                await _next(httpContext);
            }
            catch (Exception ex)
            {
                httpContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                httpContext.Response.ContentType = "application/json";
                var response = _host.IsDevelopment() ?
                    ApiResponseFactory.InternalServerError(ex.Message, new List<string> { ex.StackTrace }) :
                    ApiResponseFactory.InternalServerError(localizer["InternalServerError"].Value);
                    //new ApiExceptions((int)HttpStatusCode.InternalServerError, ex.Message, ex.StackTrace) :
                    //new ApiExceptions((int)HttpStatusCode.InternalServerError, ex.Message);

                var json = JsonSerializer.Serialize(response);
                await httpContext.Response.WriteAsync(json);
            }
        }

        // Legacy rate limiting logic (based on IMemoryCache and IP)
        // Replaced by built-in ASP.NET Core Rate Limiter using AddRateLimiter in Program.cs
        // Use app.UseRateLimiter() in the pipeline and configure it via services.AddRateLimiter()
        // NOTE: This method is no longer used and can be safely removed after verifying the new setup.
        private bool IsRequestAllowed(HttpContext context)
        {
            var ip = context.Connection.RemoteIpAddress?.ToString();
            var cacheKey = $"Rate:{ip}";
            var date = DateTime.Now;
            var (timeStamp, count) = _cache.GetOrCreate(cacheKey, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _rateLimiter;
                return (timeStamp: date, count: 0);
            });

            if (date - timeStamp < _rateLimiter)
            {
                if (count >= 8)
                {
                    return false;
                }
                _cache.Set(cacheKey, (timeStamp, count += 1), _rateLimiter);
            }
            else
            {
                _cache.Set(cacheKey, (timeStamp, count), _rateLimiter);
            }

            return true;

        }

        private void ApplySecurity(HttpContext context)
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-XSS-Protection"] = "1;mode=block";
            context.Response.Headers["X-Frame-Options"] = "DENY";
        }

    }
    /// <summary>
    /// Exception Handling Middleware Extensions
    /// </summary>
    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class ExceptionHandlingMiddlewareExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ExceptionHandlingMiddleware>();
        }
    }
}
