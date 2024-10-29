using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Invoice
{
    public partial class InvoiceTransaction : BaseEntity
    {
        public int Id { get; set; }
        public decimal ValuePay { get; set; }
        public decimal TotalCreditCardPay { get; set; }
        public string CustomerDepositeApply { get; set; }
        //public string CustomerPaymentApply { get; set; }
        //public string CustomerMemoApply { get; set; }
        public string InvoiceApply { get; set; }

        public DateTime CreatedDate { get; set; }
    }
}
