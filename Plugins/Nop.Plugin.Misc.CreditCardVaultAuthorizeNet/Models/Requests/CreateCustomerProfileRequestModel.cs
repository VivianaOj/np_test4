namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests
{
    public class CreateCustomerProfileRequestModel
    {
        public CreateCustomerProfileRequestModel()
        {
            createCustomerProfileRequest = new CreateCustomerProfileRequest();
        }

        public CreateCustomerProfileRequest createCustomerProfileRequest { get; set; }
    }
}
