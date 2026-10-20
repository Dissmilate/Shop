using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shop.Api.Entities;

namespace Shop.Api.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).HasMaxLength(200);
        builder.Property(p => p.NormalizedName).HasMaxLength(200);
        builder.HasIndex(p => p.NormalizedName).IsUnique();

        builder.Property(p => p.Price).HasPrecision(18, 2);
        builder.ToTable(t => t.HasCheckConstraint("CK_Products_Price", "\"Price\" > 0"));
    }
}