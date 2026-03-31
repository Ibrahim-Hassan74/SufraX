using Asp.Versioning;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;
using System.Text.Json;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Controller that acts as a proxy between the frontend and backend for password reset functionality.
    /// </summary>
    /// <remarks>
    /// This controller is useful when the frontend cannot directly call the backend API due to security restrictions,
    /// such as the need to hide the API key.
    /// </remarks>
    [ApiController]
    [Route("api/v{version:apiVersion}/frontend")]
    [ApiVersion(2.0)]
    public class FrontendProxyController : ControllerBase
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="FrontendProxyController"/> class.
        /// </summary>
        /// <param name="httpClientFactory">The factory used to create <see cref="HttpClient"/> instances.</param>
        /// <param name="configuration">The configuration provider used to access API settings.</param>
        /// <param name="httpContextAccessor">
        /// Provides access to the current HTTP context, allowing retrieval of request-specific data
        /// such as the scheme, host, and path for constructing full URLs.
        /// </param>

        public FrontendProxyController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IStringLocalizer<SharedResource> localizer)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _localizer = localizer;
        }
        /// <summary>
        /// Acts as a proxy endpoint that forwards a password reset request from the frontend to the backend API.
        /// </summary>
        /// <param name="request">The password reset request body sent from the frontend.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains the response returned by the backend API.
        /// Possible status codes include:
        /// - 200 OK if the reset was successful,
        /// - 4xx for client-side errors (e.g. invalid input, rate limits),
        /// - 5xx for server errors or connectivity issues.
        /// </returns>
        /// <remarks>
        /// This method reads the backend API URL and API key from the configuration, builds the full URL using the
        /// current HTTP request context, and forwards the request to the backend with the necessary headers.
        /// </remarks>

        [HttpPost("reset")]
        public async Task<IActionResult> ResetPasswordProxy([FromBody] ResetPasswordDTO request)
        {
            var apiKey = _configuration["ApiSettings:XApiKey"];
            var apiPath = _configuration["ApiSettings:BackendResetPasswordUrl"];

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return StatusCode(StatusCodes.Status500InternalServerError, _localizer["HostInfoUnavailable"].Value);

            var requestUrl = httpContext.Request;
            var baseUrl = $"{requestUrl.Scheme}://{requestUrl.Host.Value}";

            var fullApiUrl = $"{baseUrl.TrimEnd('/')}/{apiPath}";

            using (var client = _httpClientFactory.CreateClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Post, fullApiUrl)
                {
                    Content = JsonContent.Create(request)
                };
                httpRequest.Headers.Add("X-API-KEY", apiKey);

                var response = await client.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();
                return StatusCode((int)response.StatusCode, content);
            }
        }
        /// <summary>
        /// Acts as a proxy endpoint that forwards a verify reset password request from the frontend to the backend API.
        /// </summary>
        /// <param name="request">
        /// A <see cref="VerifyResetPasswordDTO"/> that contains the user's ID and the reset token.
        /// These parameters are passed from the frontend via query string.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> that contains the response from the backend.
        /// Status codes returned:
        /// - 200 OK: if the token is valid,
        /// - 400 Bad Request: if the token is invalid or expired,
        /// - 500 Internal Server Error: if the HTTP context is unavailable or another error occurs.
        /// </returns>
        /// <remarks>
        /// This endpoint constructs the backend verification URL using the current request scheme and host, 
        /// appends the API key in the headers, and relays the GET request with query parameters.
        /// </remarks>
        [HttpGet("reset/verify")]
        public async Task<IActionResult> VerifyResetPassword([FromQuery] VerifyResetPasswordDTO request)
        {
            var apiKey = _configuration["ApiSettings:XApiKey"];
            var apiPath = _configuration["ApiSettings:BackendVerifyResetPasswordUrl"];

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return StatusCode(StatusCodes.Status500InternalServerError, _localizer["HostInfoUnavailable"].Value);

            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";
            var fullBaseUrl = $"{baseUrl}/{apiPath}".TrimEnd('/');

            var urlWithParams = $"{fullBaseUrl}?userId={Uri.EscapeDataString(request.UserId)}&token={Uri.EscapeDataString(request.Token)}";

            using (var client = _httpClientFactory.CreateClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, urlWithParams);
                httpRequest.Headers.Add("X-API-KEY", apiKey);

                var response = await client.SendAsync(httpRequest);
                var content = await response.Content.ReadAsStringAsync();

                return StatusCode((int)response.StatusCode, content);
            }
        }
        /// <summary>
        /// Acts as a proxy endpoint that forwards an email confirmation request 
        /// from the frontend to the backend API.
        /// </summary>
        /// <param name="request">
        /// A <see cref="ConfirmEmailDTO"/> that contains the user's ID and confirmation token.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the backend response:
        /// - 200 OK: if the confirmation succeeded,
        /// - 400 Bad Request: if the token is invalid or expired,
        /// - 500 Internal Server Error: if the HTTP context is unavailable or another error occurs.
        /// </returns>
        /// <remarks>
        /// This method builds the backend confirmation URL using the current request scheme and host, 
        /// attaches query parameters (userId + token), and sends a GET request with the API key header.
        /// </remarks>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] ConfirmEmailDTO request)
        {
            var apiKey = _configuration["ApiSettings:XApiKey"];
            var apiPath = _configuration["ApiSettings:BackendConfirmEmailUrl"];

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Unable to determine host information.");

            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";
            var fullBaseUrl = $"{baseUrl}/{apiPath}".TrimEnd('/');

            var urlWithParams = $"{fullBaseUrl}?userId={Uri.EscapeDataString(request.UserId)}&token={Uri.EscapeDataString(request.Token)}";

            using (var client = _httpClientFactory.CreateClient())
            {
                var httpRequest = new HttpRequestMessage(HttpMethod.Get, urlWithParams);
                httpRequest.Headers.Add("X-API-KEY", apiKey);

                var response = await client.SendAsync(httpRequest);
                if (response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrWhiteSpace(request.RedirectTo))
                        return Redirect(request.RedirectTo);

                    return Redirect("/email-confirmed");
                }

                return Redirect("/email-confirm-failed");
            }
        }

        /// <summary>
        /// Acts as a proxy endpoint that forwards a login request from the frontend to the backend API.
        /// </summary>
        /// <param name="request">
        /// A <see cref="LoginDTO"/> containing the user's email and password.
        /// </param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing:
        /// - 200 OK: if the login succeeded and the JWT token is returned,  
        /// - 401 Unauthorized: if the credentials are invalid,  
        /// - 4xx/5xx: if other errors occur (e.g. backend unavailable).  
        /// </returns>
        /// <remarks>
        /// This method hides the API key from the frontend by sending the login request through the backend.  
        /// If successful, the returned JWT token is stored securely in an HTTP-only cookie and can be used by  
        /// Hangfire Dashboard or other backend services that rely on authentication.  
        /// </remarks>
        [HttpPost("login")]
        public async Task<IActionResult> ProxyLogin([FromBody] LoginDTO request)
        {
            var apiKey = _configuration["ApiSettings:XApiKey"];
            var loginPath = _configuration["ApiSettings:BackendLoginUrl"];

            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return StatusCode(StatusCodes.Status500InternalServerError, _localizer["NoHttpContext"].Value);

            var baseUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host.Value}";
            var fullApiUrl = $"{baseUrl.TrimEnd('/')}/{loginPath.TrimStart('/')}";

            using var client = _httpClientFactory.CreateClient();
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, fullApiUrl)
            {
                Content = JsonContent.Create(request)
            };
            httpRequest.Headers.Add("X-API-KEY", apiKey);

            var response = await client.SendAsync(httpRequest);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return StatusCode((int)response.StatusCode, content);

            var loginResponse = JsonSerializer.Deserialize<ApiSuccessResponse>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (loginResponse is null || !loginResponse.Success)
                return Unauthorized();

            httpContext.Response.Cookies.Append("token", $"Bearer {loginResponse.Token}", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = loginResponse.Expiration
            });

            return Ok(new { message = _localizer["LoginSuccess"].Value, user = loginResponse.UserName, success = loginResponse.Success });
        }


    }
}
