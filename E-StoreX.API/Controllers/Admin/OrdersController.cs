using Asp.Versioning;
using EStoreX.Core.DTO.Common;
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
    /// Provides functionality for managing and processing orders within the administrative context.
    /// </summary>
    /// <remarks>This controller inherits from <see cref="AdminControllerBase"/> and is intended for use in
    /// administrative scenarios where order-related operations are required. It serves as a base for implementing
    /// actions related to order management.</remarks>
    [ApiVersion(2.0)]
    public class OrdersController : AdminControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly IExportService _exportService;
        private readonly IStringLocalizer<SharedResource> _localizer;
        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class with the specified order service.
        /// </summary>
        /// <param name="orderService">The service used to manage order-related operations.</param>
        /// <param name="exportService">Service to manage files.</param>
        /// <param name="exportService">Service to manage files.</param>
        /// <param name="localizer">localizer for shared resources.</param>
        public OrdersController(IOrderService orderService, IExportService exportService, IStringLocalizer<SharedResource> localizer)
        {
            _orderService = orderService;
            _exportService = exportService;
            _localizer = localizer;
        }
        /// <summary>
        /// Retrieves all orders from the system.
        /// </summary>
        /// <returns>
        /// Returns <see cref="IEnumerable{OrderResponse}"/> containing all orders if found.  
        /// Returns <see cref="UnauthorizedResult"/> if the user is not authenticated.  
        /// Returns <see cref="NotFoundResult"/> if no orders exist.  
        /// Returns <see cref="StatusCodeResult"/> 500 if an unexpected error occurs.
        /// </returns>
        /// <response code="200">Orders retrieved successfully.</response>
        /// <response code="401">User is not authenticated.</response>
        [HttpGet]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrders()
        {
            var orders = await _orderService.GetAllOrders();
            return Ok(orders);
        }
        /// <summary>
        /// Exports all orders into the specified file format.
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
        /// <response code="200">orders exported successfully in the requested format.</response>
        /// <response code="400">Unsupported export type requested.</response>
        /// <response code="401">If the user is not authenticated.</response>
        [HttpGet("export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Export(ExportType type)
        {
            var orders = await _orderService.GetAllOrders();

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(orders), "text/csv", "orders.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(orders), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "orders.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(orders), "application/pdf", "orders.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }
        /// <summary>
        /// Retrieves a sales report for a given date range.  
        /// This allows administrators to analyze revenue, order volume, and top-selling products.
        /// </summary>
        /// <param name="startDate">The start date of the reporting period.</param>
        /// <param name="endDate">The end date of the reporting period.</param>
        /// <returns>
        /// Returns <see cref="SalesReportResponse"/> containing sales statistics for the period.  
        /// Returns 404 if no orders were found in the specified range.
        /// </returns>
        /// <response code="200">Sales report retrieved successfully.</response>
        /// <response code="400">Invalid date range provided (e.g., start date after end date).</response>
        /// <response code="404">No orders found for the specified date range.</response>
        [HttpGet("report")]
        [ProducesResponseType(typeof(SalesReportResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
        {
            if (startDate > endDate)
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidDateRange"].Value));

            var report = await _orderService.GetSalesReportAsync(startDate, endDate);

            if (report == null || report.TotalOrders == 0)
                return NotFound(ApiResponseFactory.NotFound(_localizer["NoSalesData"].Value));

            return Ok(report);
        }
        /// <summary>
        /// Exports the sales report for a given date range in the specified file format.
        /// </summary>
        /// <param name="startDate">The start date of the reporting period.</param>
        /// <param name="endDate">The end date of the reporting period.</param>
        /// <param name="type">The export file format (CSV, Excel, PDF).</param>
        /// <returns>A downloadable file containing the sales report.</returns>
        /// <response code="200">Sales report exported successfully.</response>
        /// <response code="400">Invalid date range or unsupported export type.</response>
        /// <response code="404">No sales data available for the specified period.</response>
        [HttpGet("report/export/{type}")]
        [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ExportSalesReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, ExportType type)
        {
            if (startDate > endDate)
                return BadRequest(ApiResponseFactory.BadRequest(_localizer["InvalidDateRange"].Value));

            var report = await _orderService.GetSalesReportAsync(startDate, endDate);

            if (report == null || report.TotalOrders == 0)
                return NotFound(ApiResponseFactory.NotFound(_localizer["NoSalesData"].Value));

            return type switch
            {
                ExportType.Csv => File(_exportService.ExportToCsv(new List<SalesReportResponse> { report }), "text/csv", "sales-report.csv"),
                ExportType.Excel => File(_exportService.ExportToExcel(new List<SalesReportResponse> { report }), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "sales-report.xlsx"),
                ExportType.Pdf => File(_exportService.ExportToPdf(new List<SalesReportResponse> { report }), "application/pdf", "sales-report.pdf"),
                _ => BadRequest(ApiResponseFactory.BadRequest(_localizer["UnsupportedExportType"].Value))
            };
        }


    }
}
