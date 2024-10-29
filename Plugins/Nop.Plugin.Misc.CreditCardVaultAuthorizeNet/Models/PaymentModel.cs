namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models
{
    public class PaymentModel
    {
        public PaymentModel()
        {
            creditCard = new CreditCardModel();
        }

        public CreditCardModel creditCard { get; set; }
    }
}
