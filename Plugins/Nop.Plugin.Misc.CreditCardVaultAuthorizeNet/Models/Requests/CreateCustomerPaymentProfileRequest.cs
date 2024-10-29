using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests
{
    public class CreateCustomerPaymentProfileRequest
    {
        public CreateCustomerPaymentProfileRequest()
        {
            merchantAuthentication = new MerchantAuthenticationModel();
            paymentProfile = new PaymentProfile();
        }

        public MerchantAuthenticationModel merchantAuthentication { get; set; }
        public string customerProfileId { get; set; }
        public PaymentProfile paymentProfile { get; set; }
        public string validationMode { get; set; }
    }
}
