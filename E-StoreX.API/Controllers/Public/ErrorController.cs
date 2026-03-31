using E_StoreX.API.Helper;
using Microsoft.AspNetCore.Mvc;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Controller responsible for handling error responses.  
    /// Provides a unified endpoint for returning standardized error messages 
    /// based on HTTP status codes.
    /// </summary>
    [Route("errors/{statusCode}")]
    public class ErrorController : CustomControllerBase
    {
        /// <summary>
        /// Returns an error response with the specified HTTP status code.
        /// </summary>
        /// <param name="statusCode">The HTTP status code to include in the response.</param>
        /// <returns>
        /// An <see cref="ObjectResult"/> containing a <see cref="ResponseAPI"/> 
        /// with details about the error.
        /// </returns>
        [HttpGet]
        public IActionResult Error(int statusCode)
        {
            return new ObjectResult(new ResponseAPI(statusCode));
        }
    }
}
