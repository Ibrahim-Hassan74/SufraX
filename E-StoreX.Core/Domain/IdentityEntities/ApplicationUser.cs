using Domain.Entities.Common;
using Domain.Entities.Product;
using EStoreX.Core.Domain.Entities.Orders;
using EStoreX.Core.Domain.Entities.Rating;
using Microsoft.AspNetCore.Identity;

namespace EStoreX.Core.Domain.IdentityEntities
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public string? DisplayName { get; set; }
        public Address Address { get; set; }
        public string? LastEmailConfirmationToken { get; set; }
        public string? RefreshToken { get; set; }
        //public DateTime RefreshTokenExpirationDateTime { get; set; }
        public DateTimeOffset RefreshTokenExpirationDateTime { get; set; }
        public virtual Photo? Photo { get; set; }
        public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
        public virtual ICollection<IdentityUserRole<Guid>> UserRoles { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
