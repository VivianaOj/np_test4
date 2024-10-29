using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;

namespace Nop.Data.Mapping.Customers
{
    public partial class CompanyCustomerMap : NopEntityTypeConfiguration<CompanyCustomerMapping>
    {
        #region Methods

        public override void Configure(EntityTypeBuilder<CompanyCustomerMapping> builder)
        {
            builder.ToTable(NopMappingDefaults.CompanyCustomersTable);
            builder.HasKey(mapping => new { mapping.CompanyId, mapping.CustomerId });

            builder.Property(mapping => mapping.CompanyId).HasColumnName("Company_Id");
            builder.Property(mapping => mapping.CustomerId).HasColumnName("Customer_Id");

            builder.HasOne(mapping => mapping.Company)
                .WithMany(company => company.CompanyCustomerMappings)
                .HasForeignKey(mapping => mapping.CompanyId)
                .IsRequired();

            builder.HasOne(mapping => mapping.Customer)
                .WithMany(customer => customer.CompanyCustomerMappings)
                .HasForeignKey(mapping => mapping.CustomerId)
                .IsRequired();

            builder.Ignore(mapping => mapping.Id);

            base.Configure(builder);
        }
        #endregion
    }
}
