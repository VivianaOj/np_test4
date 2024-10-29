using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;
using Nop.Data.Mapping;


namespace Nop.Data.Mapping.Customers
{
    public partial class CompanyMap : NopEntityTypeConfiguration<Company>
    {
        public override void Configure(EntityTypeBuilder<Company> builder)
        {
            builder.ToTable(nameof(Company));
            builder.HasKey(company => company.Id);

            builder.Property(company => company.BillingAddressId).HasColumnName("BillingAddress_Id");
            builder.Property(company => company.ShippingAddressId).HasColumnName("ShippingAddress_Id");

            builder.HasOne(company => company.BillingAddress)
                .WithMany()
                .HasForeignKey(company => company.BillingAddressId);

            builder.HasOne(company => company.ShippingAddress)
                .WithMany()
                .HasForeignKey(company => company.ShippingAddressId);

            builder.HasOne(company => company.PriceLevel)
             .WithMany()
             .HasForeignKey(company => company.PriceLevelId);


            builder.Ignore(company => company.Addresses);

            base.Configure(builder);
        }
    }
}
