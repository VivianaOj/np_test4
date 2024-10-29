using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Invoice
{
    public partial class InvoicePayment : BaseEntity
    {
        public int InvoiceId { get; set; }

        public decimal BalanceApply { get; set; }
        public decimal CreditCardApply { get; set; }
        public DateTime Date { get; set; }
        public int Status { get; set; }
    }
}
