using Asp.Versioning;
using EStoreX.Core.DTO.Common;
using EStoreX.Core.DTO.Discount.Request;
using EStoreX.Core.DTO.Discounts.Responses;
using EStoreX.Core.Enums;
using EStoreX.Core.Helper;
using EStoreX.Core.ServiceContracts.Common;
using EStoreX.Core.ServiceContracts.Discount;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using EStoreX.API.Filters;

namespace EStoreX.API.Controllers.Admin
{
    /// <summary>
    /// Provides endpoints for managing discounts in the system.
    /// This controller is intended for administrators only.
    /// </summary>
    [ApiVersion(2.0)]
    public class DiscountsController : AdminControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;

        /// <summary>
        /// Initializes a new instance of the <see cref="DiscountsController"/> class.
        /// </summary>
        /// <param name="discountService">Service responsible for managing discount operations, including creation, update, deletion, and validation.</param>
        /// <param name="exportService">Service responsible for exporting discount data in various formats (CSV, Excel, PDF).</param>
        /// <param name="exportService">Service responsible for exporting discount data in various formats (CSV, Excel, PDF).</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public DiscountsController(IDiscountService discountService, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _discountService = discountService;
            _exportService = exportService;
            _localizer = localizer;
        }

        /// <summary>
        /// Creates a new discount entry in the system, allowing administrators to define promotional offers 
        /// such as percentage-based or fixed-amount discounts.  
        /// </summary>
        /// <param name="request">
        /// An object containing the discount details (e.g., code, percentage, validity period, and usage rules).
        /// </param>
        /// <returns>
        /// Returns the details of the newly created discount wrapped in <see cref="ApiResponseWithData{T}"/> 
        /// if the operation succeeds, or an <see cref="ApiResponse"/> with validation errors if the request is invalid.
        /// </returns>
        /// <response code="200">The discount was created successfully and returned in the response body.</response>
        /// <response code="400">The provided discount details were invalid or incomplete.</response>
        /// <remarks>
        /// Example request:  
        /// POST /api/discount  
        /// {
        ///   "code": "SUMMER25",
        ///   "percentage": 25,
        ///   "startDate": "2025-09-01T00:00:00Z",
        ///   "endDate": "2025-09-30T23:59:59Z",
        ///   "maxUsage": 100
        /// }
        /// </remarks>

        [HttpPost]
        [ProducesResponseType(typeof(ApiResponseWithData<DiscountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] DiscountRequest request)
        {
            var result = await _discountService.CreateDiscountAsync(request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the details of an existing discount in the system.  
        /// This allows administrators to modify attributes such as the discount code, value, validity period,
        /// or usage rules without creating a new discount entry.
        /// </summary>
        /// <param name="id">The unique identifier of the discount to update.</param>
        /// <param name="request">
        /// An object containing the updated discount details (e.g., code, percentage or amount, start and end dates, usage limits).
        /// </param>
        /// <returns>
        /// Returns the updated discount information wrapped in <see cref="ApiResponseWithData{T}"/> 
        /// if the operation succeeds, or an <see cref="ApiResponse"/> indicating that the discount was not found.
        /// </returns>
        /// <response code="200">The discount was successfully updated and the updated details are returned.</response>
        /// <response code="404">No discount with the specified <c>id</c> was found.</response>
        /// <remarks>
        /// Example request:  
        /// PUT /api/discount/{id}  
        /// {
        ///   "code": "WINTER10",
        ///   "percentage": 10,
        ///   "startDate": "2025-12-01T00:00:00Z",
        ///   "endDate": "2025-12-31T23:59:59Z",
        ///   "maxUsage": 50
        /// }
        /// </remarks>
        [HttpPut("{id}")]
        [ProducesResponseType(typeof(ApiResponseWithData<DiscountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] DiscountRequest request)
        {
            var result = await _discountService.UpdateDiscountAsync(id, request);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Permanently removes an existing discount from the system.  
        /// This action is typically used by administrators when a discount code is no longer valid 
        /// or should no longer be available for use.
        /// </summary>
        /// <param name="id">The unique identifier of the discount to delete.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating whether the delete operation succeeded or if the discount was not found.
        /// </returns>
        /// <response code="200">The discount was successfully deleted.</response>
        /// <response code="404">No discount with the specified <c>id</c> was found.</response>
        /// <remarks>
        /// Example request:  
        /// DELETE /api/discount/{id}  
        /// </remarks>

        [HttpDelete("{id}")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _discountService.DeleteDiscountAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Activates an existing discount immediately, making it available for use by customers.  
        /// This action is typically performed by administrators to enable a discount code without waiting
        /// for its scheduled start date.
        /// </summary>
        /// <param name="id">The unique identifier of the discount to activate.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating the outcome of the operation (success or not found).
        /// </returns>
        /// <response code="200">The discount was successfully activated and is now available for use.</response>
        /// <response code="404">No discount with the specified <c>id</c> was found.</response>
        /// <remarks>
        /// Example request:  
        /// POST /api/discount/{id}/activate  
        /// </remarks>

        [HttpPatch("{id}/activate")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid id)
        {
            var result = await _discountService.ActivateDiscountAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Expires an existing discount immediately, preventing it from being used in any future orders.  
        /// This action is typically performed by administrators to disable a discount code before its scheduled end date.
        /// </summary>
        /// <param name="id">The unique identifier of the discount to expire.</param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating the result of the operation (success or not found).
        /// </returns>
        /// <response code="200">The discount was successfully expired and is no longer available for use.</response>
        /// <response code="404">No discount with the specified <c>id</c> was found.</response>
        /// <remarks>
        /// Example request:  
        /// POST /api/discount/{id}/expire  
        /// </remarks>
        [HttpPatch("{id}/expire")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Expire(Guid id)
        {
            var result = await _discountService.ExpireDiscountAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Updates the validity period of an existing discount by modifying its start and end dates.  
        /// This action is typically used by administrators to extend, shorten, or reschedule a discount campaign.
        /// </summary>
        /// <param name="id">The unique identifier of the discount to update.</param>
        /// <param name="request">
        /// An object containing the new start and end dates for the discount's validity period.
        /// </param>
        /// <returns>
        /// An <see cref="ApiResponse"/> indicating the outcome of the operation (success or not found).
        /// </returns>
        /// <response code="200">The discount validity dates were successfully updated.</response>
        /// <response code="404">No discount with the specified <c>id</c> was found.</response>
        /// <remarks>
        /// Example request:  
        /// PUT /api/discount/{id}/update-dates  
        /// {
        ///   "startDate": "2025-10-01T00:00:00Z",
        ///   "endDate": "2025-12-31T23:59:59Z"
        /// }
        /// </remarks>
        [HttpPatch("{id}/dates")]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateDates(Guid id, [FromBody] UpdateDiscountDatesRequest request)
        {
            var result = await _discountService.UpdateDiscountDatesAsync(id, request.StartDate, request.EndDate);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves the details of a specific discount by its unique identifier.  
        /// This allows administrators or authorized users to view information such as the discount code, value, 
        /// validity period, and usage rules.
        /// </summary>
        /// <param name="id">The unique identifier of the discount to retrieve.</param>
        /// <returns>
        /// A <see cref="DiscountResponse"/> wrapped in <see cref="ApiResponseWithData{T}"/> if the discount exists,  
        /// or an <see cref="ApiResponse"/> with a not found message if it does not.
        /// </returns>
        /// <response code="200">The discount was found and its details are returned in the response.</response>
        /// <response code="404">No discount with the specified <c>id</c> was found.</response>
        /// <remarks>
        /// Example request:  
        /// GET /api/discount/{id}  
        /// </remarks>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(ApiResponseWithData<DiscountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _discountService.GetDiscountByIdAsync(id);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves the details of a discount using its unique code.  
        /// This allows customers or administrators to check whether a discount code is valid 
        /// and view its associated details such as value, type, and validity period.
        /// </summary>
        /// <param name="code">The unique discount code provided by the customer.</param>
        /// <returns>
        /// A <see cref="DiscountResponse"/> wrapped in <see cref="ApiResponseWithData{T}"/> if the discount exists,  
        /// or an <see cref="ApiResponse"/> with an error message if the discount code is invalid or not found.
        /// </returns>
        /// <response code="200">The discount was found and its details are returned in the response.</response>
        /// <response code="404">No discount with the specified <c>code</c> was found.</response>
        /// <remarks>
        /// Example request:  
        /// GET /api/discount/by-code/SUMMER25  
        /// </remarks>

        [HttpGet("code/{code}")]
        [ProducesResponseType(typeof(ApiResponseWithData<DiscountResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByCode(string code)
        {
            var result = await _discountService.GetDiscountByCodeAsync(code);
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves a list of all discounts available in the system.  
        /// This includes active, expired, and scheduled discounts, allowing administrators 
        /// to manage and review the entire discount catalog.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="DiscountResponse"/> objects wrapped in 
        /// <see cref="ApiResponseWithData{T}"/>.
        /// </returns>
        /// <response code="200">All discounts were retrieved successfully.</response>
        /// <remarks>
        /// Example request:  
        /// GET /api/discount  
        /// </remarks>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponseWithData<List<DiscountResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _discountService.GetAllDiscountsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves a list of all discounts that are currently active and available for use.  
        /// Active discounts are those within their validity period and not expired or deactivated.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="DiscountResponse"/> objects wrapped in 
        /// <see cref="ApiResponseWithData{T}"/> representing the active discounts.
        /// </returns>
        /// <response code="200">The list of active discounts was retrieved successfully.</response>
        /// <remarks>
        /// Example request:  
        /// GET /api/discount/active  
        /// </remarks>
        [HttpGet("active")]
        [ProducesResponseType(typeof(ApiResponseWithData<List<DiscountResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetActive()
        {
            var result = await _discountService.GetActiveDiscountsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves a list of all discounts that have expired.  
        /// Expired discounts are those whose end date has passed or that were manually expired 
        /// and are no longer valid for use.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="DiscountResponse"/> objects wrapped in 
        /// <see cref="ApiResponseWithData{T}"/> representing the expired discounts.
        /// </returns>
        /// <response code="200">The list of expired discounts was retrieved successfully.</response>
        /// <remarks>
        /// Example request:  
        /// GET /api/discount/expired  
        /// </remarks>
        [HttpGet("expired")]
        [ProducesResponseType(typeof(ApiResponseWithData<List<DiscountResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetExpired()
        {
            var result = await _discountService.GetExpiredDiscountsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Retrieves a list of all discounts that are scheduled but have not yet started.  
        /// Upcoming discounts are those whose start date is in the future, making them unavailable 
        /// until the scheduled activation date.
        /// </summary>
        /// <returns>
        /// A collection of <see cref="DiscountResponse"/> objects wrapped in 
        /// <see cref="ApiResponseWithData{T}"/> representing the upcoming discounts.
        /// </returns>
        /// <response code="200">The list of upcoming discounts was retrieved successfully.</response>
        /// <remarks>
        /// Example request:  
        /// GET /api/discount/upcoming  
        /// </remarks>
        [HttpGet("upcoming")]
        [ProducesResponseType(typeof(ApiResponseWithData<List<DiscountResponse>>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetNotStarted()
        {
            var result = await _discountService.GetNotStartedDiscountsAsync();
            return StatusCode(result.StatusCode, result);
        }

        /// <summary>
        /// Exports all currently active discounts in the specified format (CSV, Excel, or PDF).  
        /// Use this endpoint to download active discounts for reporting or analysis purposes.  
        /// </summary>
        /// <param name="type">The type of export format (Csv, Excel, Pdf).</param>
        [HttpGet("export/active/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportActiveDiscounts(ExportType type)
        {
            var discounts = await _discountService.GetActiveDiscountsAsync() as ApiResponseWithData<List<DiscountResponse>>;
            if (discounts is null) 
                return NotFound(ApiResponseFactory.NotFound(_localizer["DiscountNotFound"].Value));
            return ExportDiscountsFile(discounts.Data, type, "active");
        }

        /// <summary>
        /// Exports all upcoming (not yet started) discounts in the specified format (CSV, Excel, or PDF).  
        /// Use this endpoint to download upcoming discounts for planning and promotional purposes.  
        /// </summary>
        /// <param name="type">The type of export format (Csv, Excel, Pdf).</param>
        [HttpGet("export/upcoming/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportUpcomingDiscounts(ExportType type)
        {
            var discounts = await _discountService.GetNotStartedDiscountsAsync() as ApiResponseWithData<List<DiscountResponse>>;
            if (discounts is null)
                return NotFound(ApiResponseFactory.NotFound());
            return ExportDiscountsFile(discounts.Data, type, "upcoming");
        }

        /// <summary>
        /// Exports all expired discounts in the specified format (CSV, Excel, or PDF).  
        /// Use this endpoint to review past discounts for auditing or historical analysis.  
        /// </summary>
        /// <param name="type">The type of export format (Csv, Excel, Pdf).</param>
        [HttpGet("export/expired/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportExpiredDiscounts(ExportType type)
        {
            var discounts = await _discountService.GetExpiredDiscountsAsync() as ApiResponseWithData<List<DiscountResponse>>;
            if (discounts is null)
                return NotFound(ApiResponseFactory.NotFound()); ;
            return ExportDiscountsFile(discounts.Data, type, "expired");
        }

        /// <summary>
        /// Exports all discounts (active, upcoming, and expired) in the specified format (CSV, Excel, or PDF).  
        /// Restricted to <b>Admin users</b> for reporting or auditing purposes.  
        /// </summary>
        /// <param name="type">The type of export format (Csv, Excel, Pdf).</param>
        [HttpGet("export/all/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ExportAllDiscounts(ExportType type)
        {
            var discounts = await _discountService.GetAllDiscountsAsync() as ApiResponseWithData<List<DiscountResponse>>;
            if (discounts is null)
                return NotFound(ApiResponseFactory.NotFound());
            return ExportDiscountsFile(discounts.Data, type, "all");
        }

        private IActionResult ExportDiscountsFile(List<DiscountResponse> discounts, ExportType type, string suffix)
        {
            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(discounts), "text/csv", $"discounts_{suffix}.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(discounts), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"discounts_{suffix}.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(discounts), "application/pdf", $"discounts_{suffix}.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }
    }
}
