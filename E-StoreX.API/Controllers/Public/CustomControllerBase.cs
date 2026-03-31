using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace EStoreX.API.Controllers.Public
{
    /// <summary>
    /// Base controller for the E-StoreX API, providing common functionality for all controllers.
    /// </summary>
    [ApiController]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CustomControllerBase : ControllerBase
    {
        /// <summary>
        /// Constructor for CustomControllerBase, initializing the unit of work.
        /// </summary>

        public CustomControllerBase()
        {   
            // This constructor can be used to initialize common services or properties
            // that all derived controllers might need.
        }
    }
}
