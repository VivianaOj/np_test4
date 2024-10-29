namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests
{
    public class CreateCustomerPaymentProfileRequestModel
    {
        public CreateCustomerPaymentProfileRequestModel()
        {
            createCustomerPaymentProfileRequest = new CreateCustomerPaymentProfileRequest();
        }

        public CreateCustomerPaymentProfileRequest createCustomerPaymentProfileRequest { get; set; }
    }
}
