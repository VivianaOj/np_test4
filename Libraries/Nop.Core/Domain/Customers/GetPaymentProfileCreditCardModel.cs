using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class GetPaymentProfileCreditCardModel
    {
        public string cardNumber { get; set; }
        public string expirationDate { get; set; }
        public string cardType { get; set; }
        public string issuerNumber { get; set; }
    }
}
