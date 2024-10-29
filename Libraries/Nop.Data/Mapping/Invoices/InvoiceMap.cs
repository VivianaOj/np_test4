using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Invoices;

namespace Nop.Data.Mapping.Invoices
{
    public partial class InvoiceMap : NopEntityTypeConfiguration<Nop.Core.Domain.Invoices.Invoice>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Nop.Core.Domain.Invoices.Invoice> builder)
        {
            builder.ToTable(nameof(Invoice));
            builder.HasKey(order => order.Id);

            builder.Property(order => order.CreatedDate).HasColumnType("datetime2");
            builder.Property(order => order.PostingPeriod).HasColumnType("nvarchar(-1)");
            builder.Property(order => order.Location).HasColumnType("nvarchar(-1)");
            builder.Property(order => order.PONumber).HasColumnType("nvarchar(-1)");
            builder.Property(order => order.PlacedByName).HasColumnType("nvarchar(-1)");
            builder.Property(order => order.ShippingId).HasColumnType("int");
            builder.Property(order => order.SaleOrderId).HasColumnType("int");
            builder.Property(order => order.CustomerId).HasColumnType("int");
            builder.Property(order => order.Subtotal).HasColumnType("decimal(9, 18)");
            builder.Property(order => order.DiscountItem).HasColumnType("decimal(9, 18)");
            builder.Property(order => order.TaxTotal).HasColumnType("decimal(9, 18)");
            builder.Property(order => order.ShippingCost).HasColumnType("decimal(9, 18)");
            builder.Property(order => order.Total).HasColumnType("decimal(9, 18)");
            builder.Property(order => order.AmountDue).HasColumnType("decimal(9, 18)");
            builder.Property(order => order.InvoiceNo).HasColumnType("varchar(50)");

            base.Configure(builder);
        }

        #endregion
    }
}
