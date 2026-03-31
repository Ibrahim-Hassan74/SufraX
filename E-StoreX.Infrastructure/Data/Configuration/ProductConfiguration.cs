using Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EStoreX.Infrastructure.Data.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.Property(x => x.NameEn).IsRequired();
            builder.Property(x => x.NameAr).IsRequired();
            builder.Property(x => x.DescriptionEn).IsRequired();
            builder.Property(x => x.DescriptionAr).IsRequired();

            builder.Property(x => x.NewPrice).HasColumnType("decimal(18, 2)").IsRequired();
            builder.Property(x => x.OldPrice).HasColumnType("decimal(18, 2)").IsRequired();
            builder.Property(x => x.SalesCount).HasDefaultValue(0);
            builder.Property(x => x.IsFeatured).HasDefaultValue(false);

            //builder.HasData(new Product
            //{
            //    Id = Guid.Parse("A1B2C3D4-E5F6-7890-ABCD-EF1234567890"),
            //    NameEn = "Sample Product",
            //    DescriptionEn = "This is a sample product description.",
            //    NewPrice = 19.99m,
            //    CategoryId = Guid.Parse("19F389FE-8472-46FC-83EA-2440790A2067")
            //});
        }
    }
}
