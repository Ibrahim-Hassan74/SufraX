using System;
using System.ComponentModel.DataAnnotations;

namespace EStoreX.Core.DTO.Products.Requests
{
    public class ProductUpdateRequest : ProductAddRequest
    {
        /// <summary>
        /// Gets or sets the unique identifier for the product.
        /// </summary>
        [Required(ErrorMessageResourceName = "RequiredProductId", ErrorMessageResourceType = typeof(EStoreX.Core.Resources.DTO.Products.ProductValidationMessages))]
        public Guid Id { get; set; }
    }
}
