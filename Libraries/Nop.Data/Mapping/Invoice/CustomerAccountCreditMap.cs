using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Invoice
{
    public partial class CustomerAccountCreditMap: NopEntityTypeConfiguration<CustomerAccountCredit>
    {
        public override void Configure(EntityTypeBuilder<CustomerAccountCredit> builder)
        {
            builder.ToTable(nameof(CustomerAccountCredit));
            builder.HasKey(r => r.Id);

           
            base.Configure(builder);
        }
    }
   
}
