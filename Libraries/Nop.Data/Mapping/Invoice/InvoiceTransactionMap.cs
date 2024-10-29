using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Invoice
{

    public partial class InvoiceTransactionMap : NopEntityTypeConfiguration<InvoiceTransaction>
    {
        public override void Configure(EntityTypeBuilder<InvoiceTransaction> builder)
        {
            builder.ToTable(nameof(InvoiceTransaction));
            builder.HasKey(InvoicePayment => InvoicePayment.Id);


            base.Configure(builder);
        }
    }

}
