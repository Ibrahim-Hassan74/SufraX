using Asp.Versioning;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Orders.Requests;
using EStoreX.Core.DTO.Orders.Responses;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Orders;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// DeliveryMethodController handles admin operations related to delivery methods.
    /// </summary>
    [Route("api/v{version:apiVersion}/admin/delivery-method")]
    [ApiVersion(2.0)]
    public class DeliveryMethodController : AdminControllerBase
    {
        private readonly IDeliveryMethodService _deliveryMethod;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="DeliveryMethodController"/> class.
        /// </summary>
        /// <param name="localizer">localizer for shared resources.</param>
        public DeliveryMethodController(IDeliveryMethodService deliveryMethod, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _deliveryMethod = deliveryMethod;
            _exportService = exportService;
            _localizer = localizer;
        }
        /// <summary>
        /// Creates a new delivery method.
        /// </summary>
        /// <param name="request">The delivery method details.</param>
        /// <returns>The created delivery method.</returns>
        /// <response code="201">Delivery method created successfully.</response>
        /// <response code="400">Invalid request data.</response>
        /// <response code="401">Unauthorized user.</response>
        [HttpPost]
        [ProducesResponseType(typeof(DeliveryMethodResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] DeliveryMethodRequest request)
        {
            var created = await _deliveryMethod.CreateAsync(request);
            return Ok(created);
        }

        /// <summary>
        /// Updates an existing delivery method.
        /// </summary>
        /// <param name="id">The unique identifier of the delivery method to update.</param>
        /// <param name="request">The updated delivery method details.</param>
        /// <returns>The updated delivery method.</returns>
        /// <response code="200">Delivery method updated successfully.</response>
        /// <response code="404">Delivery method not found.</response>
        /// <response code="401">Unauthorized user.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(DeliveryMethodResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Update(Guid id, [FromBody] DeliveryMethodRequest request)
        {
            var updated = await _deliveryMethod.UpdateAsync(id, request);
            if (updated == null)
                return NotFound(ApiResponseFactory.NotFound(_localizer["DeliveryMethodNotFound"].Value));
            return Ok(updated);
        }

        /// <summary>
        /// Deletes a delivery method by its unique identifier.
        /// </summary>
        /// <param name="id">The unique identifier of the delivery method to delete.</param>
        /// <returns>No content if deletion succeeded.</returns>
        /// <response code="204">Delivery method deleted successfully.</response>
        /// <response code="404">Delivery method not found.</response>
        /// <response code="401">Unauthorized user.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var deleted = await _deliveryMethod.DeleteAsync(id);
            if (!deleted)
                return NotFound(ApiResponseFactory.NotFound());
            return NoContent();
        }
        /// <summary>
        /// Exports all deliveryMethods into the specified file format.
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
        /// <response code="200">deliveryMethods exported successfully in the requested format.</response>
        /// <response code="400">Unsupported export type requested.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Export(ExportType type)
        {
            var deliveryMethods = await _deliveryMethod.GetAllAsync();

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(deliveryMethods), "text/csv", "delivery-methods.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(deliveryMethods), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "delivery-methods.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(deliveryMethods), "application/pdf", "delivery-methods.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }
    }
}