using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Invoice
{
    public partial class InvoicePaymentMap : NopEntityTypeConfiguration<InvoicePayment>
    {
        public override void Configure(EntityTypeBuilder<InvoicePayment> builder)
        {
            builder.ToTable(nameof(InvoicePayment));
            builder.HasKey(InvoicePayment => InvoicePayment.Id);

           
            base.Configure(builder);
        }
    }
   
}
