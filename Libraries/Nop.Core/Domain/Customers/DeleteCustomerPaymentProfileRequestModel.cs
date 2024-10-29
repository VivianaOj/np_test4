using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class deleteCustomerPaymentProfile
    {
        public deleteCustomerPaymentProfile()
        {
            deleteCustomerPaymentProfileRequest = new DeleteCustomerPaymentProfileRequestModel();
        }

        public DeleteCustomerPaymentProfileRequestModel deleteCustomerPaymentProfileRequest { get; set; }
    }


    public class DeleteCustomerPaymentProfileRequestModel
    {
        public DeleteCustomerPaymentProfileRequestModel()
        {
            merchantAuthentication = new MerchantAuthenticationModel();
        }

        public MerchantAuthenticationModel merchantAuthentication { get; set; }
        public string customerProfileId { get; set; }
        public string customerPaymentProfileId { get; set; }
    }

    public class merchantAuthenticationModelRequest
    {
        public merchantAuthenticationModelRequest()
        {
            merchantAuthentication = new MerchantAuthenticationModel();
        }

        public MerchantAuthenticationModel merchantAuthentication { get; set; }
       
    }
}
