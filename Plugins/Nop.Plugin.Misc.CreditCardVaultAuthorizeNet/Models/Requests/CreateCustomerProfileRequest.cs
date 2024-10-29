using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests
{
    public class CreateCustomerProfileRequest
    {
        public CreateCustomerProfileRequest()
        {
            merchantAuthentication = new MerchantAuthenticationModel();
            profile = new ProfileModel();
        }

        public MerchantAuthenticationModel merchantAuthentication { get; set; }
        public ProfileModel profile { get; set; }
        public string validationMode { get; set; }
    }
}
