using EStoreX.Core.Domain.Options;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;

namespace EStoreX.API.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    /// <summary>
    /// html rewrite middleware
    /// </summary>
    public class HtmlRewriteMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly Dictionary<string, string> _mappings;
        /// <summary>
        /// html rewrite middleware constructor
        /// </summary>
        /// <param name="next">call next middleware</param>
        /// <param name="options">html redirect paths</param>
        public HtmlRewriteMiddleware(RequestDelegate next, IOptions<HtmlRewriteOptions> options)
        {
            _next = next;
            _mappings = options.Value.Mappings;
        }
        /// <summary>
        /// html rewrite middleware invoke method
        /// </summary>
        /// <param name="context">current context</param>
        /// <returns>call next</returns>
        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value?.ToLower();

            if (!string.IsNullOrEmpty(path) && _mappings.ContainsKey(path))
                context.Request.Path = _mappings[path];

            await _next(context);
        }
    }

    // Extension method used to add the middleware to the HTTP request pipeline.
    /// <summary>
    /// html rewrite middleware extensions
    /// </summary>
    public static class HtmlRewriteMiddlewareExtensions
    {
        /// <summary>
        /// html rewrite middleware extension method
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseHtmlRewriteMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<HtmlRewriteMiddleware>();
        }
    }
}
