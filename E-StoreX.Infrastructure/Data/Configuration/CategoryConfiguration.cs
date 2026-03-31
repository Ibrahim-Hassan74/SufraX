using Domain.Entities.Product;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EStoreX.Infrastructure.Data.Configuration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.NameEn)
                .IsRequired()
                .HasMaxLength(50);

            //builder.Property(c => c.DescriptionEn)
            //    .HasMaxLength(300);

            builder.HasMany(c => c.Products)
                .WithOne(p => p.Category)
                .HasForeignKey(p => p.CategoryId);

            //builder.HasData(
            //    new Category
            //    {
            //        Id = Guid.Parse("19F389FE-8472-46FC-83EA-2440790A2067"),
            //        NameEn = "Electronics",
            //        DescriptionEn = "Devices and gadgets"
            //    }
            //    );
        }
    }
}
