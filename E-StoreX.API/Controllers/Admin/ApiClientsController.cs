using Asp.Versioning;
using Domain.Entities.Common;
using EStoreX.Core.DTO.Account.Requests;
using EStoreX.Core.DTO.Account.Responses;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Account;
using EStoreX.Core.ServiceContracts.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// Provides administrative operations for managing API clients in the E-StoreX platform.
    /// </summary>
    /// <remarks>
    /// This controller is restricted to <b>Admin users</b> only and supports API version 2.0.  
    /// It is responsible for managing API client configurations, authentication keys, and permissions.  
    /// <para>
    /// Inherits from <see cref="AdminControllerBase"/>.  
    /// </para>
    /// </remarks>
    [ApiVersion(2.0)]
    public class ApiClientsController : AdminControllerBase
    {
        private readonly IApiClientService _clientService;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiClientsController"/> class with the specified API client
        /// service.
        /// </summary>
        /// <remarks>— This constructor is typically used by dependency injection to provide the required
        /// service for API client management.</remarks>
        /// <param name="clientService">The service used to manage API client data and operations. Cannot be <c>null</c>.</param>
        /// <param name="exportService">Service to manage files.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public ApiClientsController(IApiClientService clientService, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _clientService = clientService;
            _exportService = exportService;
            _localizer = localizer;
        }

        /// <summary>
        /// Registers a new API client and returns its API key.
        /// </summary>
        /// <param name="request">The request object containing client registration details.</param>
        /// <returns>
        /// Returns <see cref="RegisterApiClientResponse"/> if the client was created successfully.  
        /// Returns <see cref="BadRequestResult"/> if the request is invalid.  
        /// </returns>
        /// <response code="200">Client registered successfully.</response>
        /// <response code="400">Invalid client data provided.</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterApiClientResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterApiClientResponse>> RegisterClient([FromBody] RegisterApiClientRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ClientName))
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["ClientNameRequired"].Value));

            var client = await _clientService.CreateClientAsync(request.ClientName);

            return Ok(new RegisterApiClientResponse
            {
                ClientId = client.Id,
                ClientName = client.ClientName,
                ApiKey = client.ApiKey
            });
        }

        /// <summary>
        /// Retrieves client details by its API key.
        /// </summary>
        /// <param name="apiKey">The API key used to identify the client.</param>
        /// <returns>
        /// Returns <see cref="ApiClient"/> if the API key is valid.  
        /// Returns <see cref="NotFoundResult"/> if no client exists for the given key.  
        /// </returns>
        /// <response code="200">Client found successfully.</response>
        /// <response code="404">Client not found.</response>
        [HttpGet("client/{apiKey}")]
        [ProducesResponseType(typeof(ApiClient), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClientByKey(string apiKey)
        {
            var res = await _clientService.GetByApiKeyAsync(apiKey);
            return Ok(res);
        }
        /// <summary>
        /// Retrieves all registered API clients.
        /// </summary>
        /// <returns>Returns a list of <see cref="ApiClient"/> entities.</returns>
        /// <response code="200">Clients retrieved successfully.</response>
        [HttpGet("all")]
        [ProducesResponseType(typeof(IEnumerable<ApiClient>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllClients()
        {
            var res = await _clientService.GetClientsAsync();
            return Ok(res);
        }

        /// <summary>
        /// Activates an API client by its unique identifier.
        /// </summary>
        /// <param name="clientId">The unique identifier of the API client to activate.</param>
        /// <returns>
        /// Returns success message if activation succeeded.  
        /// Returns <see cref="NotFoundResult"/> if client is not found.  
        /// </returns>
        /// <response code="200">Client activated successfully.</response>
        /// <response code="404">Client not found.</response>
        [HttpPatch("activate/{clientId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> ActiveClient(Guid clientId)
        {
            var result = await _clientService.ActiveClientAsync(clientId);
            if (!result) return ApiResponseFactory.NotFound(_localizer["ClientNotFound"].Value);
            return ApiResponseFactory.Success(_localizer["ClientActivated"].Value);
        }

        /// <summary>
        /// Deactivates an API client by its unique identifier.
        /// </summary>
        /// <param name="clientId">The unique identifier of the API client to deactivate.</param>
        /// <returns>Returns success message if deactivation succeeded; otherwise 404.</returns>
        /// <response code="200">Client deactivated successfully.</response>
        /// <response code="404">Client not found.</response>
        [HttpPatch("deactivate/{clientId}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeActivateClient(Guid clientId)
        {
            var result = await _clientService.DeActivateClientAsync(clientId);
            if (!result) return ApiResponseFactory.NotFound(_localizer["ClientNotFound"].Value);
            return ApiResponseFactory.Success(_localizer["ClientDeactivated"].Value);
        }

        /// <summary>
        /// Retrieves a specific API client by its unique identifier.
        /// </summary>
        /// <param name="clientId">The unique identifier of the API client.</param>
        /// <returns>Returns <see cref="ApiClient"/> if found; otherwise 404.</returns>
        /// <response code="200">Client retrieved successfully.</response>
        /// <response code="404">Client not found.</response>
        [HttpGet("{clientId}")]
        [ProducesResponseType(typeof(ApiClient), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetClient(Guid clientId)
        {
            var client = await _clientService.GetClientAsync(clientId);
            if (client == null) return NotFound(ApiResponseFactory.NotFound(_localizer["ClientNotFound"].Value));
            return Ok(client);
        }
        /// <summary>
        /// Removes an API client by its unique identifier.
        /// </summary>
        /// <param name="clientId">The unique identifier of the client to remove.</param>
        /// <returns>Returns success message if removal succeeded; otherwise 404.</returns>
        /// <response code="200">Client removed successfully.</response>
        /// <response code="404">Client not found.</response>
        [HttpDelete("{clientId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RemoveClient(Guid clientId)
        {
            var result = await _clientService.RemoveClientAsync(clientId);
            if (!result) return NotFound(ApiResponseFactory.NotFound(_localizer["ClientNotFound"].Value));
            return Ok(ApiResponseFactory.Success(_localizer["ClientRemoved"].Value));
        }
        /// <summary>
        /// Updates an API client by its unique identifier.
        /// </summary>
        /// <param name="clientId">The unique identifier of the client.</param>
        /// <param name="request">The updated client data.</param>
        /// <returns>Returns success message if update succeeded; otherwise 404.</returns>
        /// <response code="200">Client updated successfully.</response>
        /// <response code="404">Client not found.</response>
        [HttpPut("{clientId:guid}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateClient([FromRoute] Guid clientId, [FromBody] UpdateClientRequest request)
        {
            var result = await _clientService.UpdateClientAsync(clientId, request);

            if (!result)
                return NotFound(ApiResponseFactory.NotFound(_localizer["ClientNotFound"].Value));

            return Ok(ApiResponseFactory.Success(_localizer["ClientUpdated"].Value));
        }

        /// <summary>
        /// Rotates the API key of a specific client.
        /// </summary>
        /// <param name="clientId">The ID of the client.</param>
        /// <returns>Returns success message with new API key if rotation succeeded; otherwise 404.</returns>
        /// <response code="200">API key rotated successfully.</response>
        /// <response code="404">Client not found.</response>
        [HttpPost("{clientId}/rotate-api-key")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> RotateApiKey(Guid clientId)
        {
            var client = await _clientService.RotateApiKeyAsync(clientId);
            if (client == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["ClientNotFound"].Value));

            return Ok(ApiResponseFactory.Success(_localizer["ApiKeyRotated"].Value, new { client.ApiKey }));
        }

        /// <summary>
        /// Exports all clients into the specified file format.
        /// </summary>
        /// <param name="type">
        /// The type of export format.  
        /// Supported values are:  
        /// <list type="bullet">
        ///   <item><description><see cref="ExportType.Csv"/> → Comma Separated Values file (.csv)</description></item>
        ///   <item><description><see cref="ExportType.Excel"/> → Microsoft Excel file (.xlsx)</description></item>
        ///   <item><description><see cref="ExportType.Pdf"/> → Portable Document Format file (.pdf)</description></item>
        /// </list>
        /// </param>
        /// <returns>
        /// A downloadable file in the selected export format.  
        /// Returns <see cref="BadRequestResult"/> if the format is not supported.
        /// </returns>
        /// <response code="200">clients exported successfully in the requested format.</response>
        /// <response code="400">Unsupported export type requested.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Export(ExportType type)
        {
            var clients = await _clientService.GetClientsAsync();

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(clients), "text/csv", "clients.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(clients), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "clients.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(clients), "application/pdf", "clients.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }


    }
}
