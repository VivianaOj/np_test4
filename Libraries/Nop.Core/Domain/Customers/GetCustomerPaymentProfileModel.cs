using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class GetCustomerPaymentProfileModel
    {
        public MerchantAuthenticationModel merchantAuthentication { get; set; }
        public string customerProfileId { get; set; }
        public bool unmaskExpirationDate { get; set; }
        public string includeIssuerInfo { get; set; }

    }
}
