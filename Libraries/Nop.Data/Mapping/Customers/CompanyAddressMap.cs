using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;

namespace Nop.Data.Mapping.Customers
{
    public partial class CompanyAddressMap : NopEntityTypeConfiguration<CompanyAddresses>
    {
        public override void Configure(EntityTypeBuilder<CompanyAddresses> builder)
        {
            builder.ToTable(NopMappingDefaults.CompanyAddressesTable);
            builder.HasKey(mapping => new { mapping.CompanyId, mapping.AddressId });

            builder.Property(mapping => mapping.CompanyId).HasColumnName("Company_Id");
            builder.Property(mapping => mapping.AddressId).HasColumnName("Address_Id");

            builder.HasOne(mapping => mapping.Company)
                .WithMany(address => address.CompanyAddressMappings)
                .HasForeignKey(mapping => mapping.CompanyId)
                .IsRequired();

            builder.HasOne(mapping => mapping.Address)
                .WithMany()
                .HasForeignKey(mapping => mapping.AddressId)
                .IsRequired();

            builder.Ignore(mapping => mapping.Id);

            base.Configure(builder);
        }
    }
}
