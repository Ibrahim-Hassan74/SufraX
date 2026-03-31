using Asp.Versioning;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Account;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.Services.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// the ContactController handles contact-related API requests.
    /// </summary>
    [ApiVersion(1.0)]
    public class ContactController : CustomControllerBase
    {
        private readonly IEmailSenderService _emailSenderService;
        private readonly IApiClientService _apiClientService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        public ContactController(IEmailSenderService emailSenderService, IApiClientService apiClientService, IStringLocalizer<SharedResource> localizer)
        {
            _emailSenderService = emailSenderService;
            _apiClientService = apiClientService;
            _localizer = localizer;
        }

        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [HttpPost("send-message")]
        public async Task<IActionResult> SendMessage([FromBody] ContactMessageDTO contactMessage)
        {
            var clientKey = HttpContext.Request.Headers["X-API-KEY"].FirstOrDefault();
            var client = await _apiClientService.GetByApiKeyAsync(clientKey!);
            var html = EmailTemplateService.GetContactMessageTemplate(contactMessage.Name, contactMessage.Email, contactMessage.Subject, contactMessage.Message);
            var email = client?.Email;
            var emailDto = new EmailDTO(email, contactMessage.Subject, html);
            await _emailSenderService.SendEmailAsync(emailDto);

            return Ok(ApiResponseFactory.Success(_localizer["MessageSent"].Value));
        }
    }

}