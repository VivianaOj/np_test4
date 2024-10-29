using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models
{
    public class PaymentProfilesModel
    {
        public PaymentProfilesModel()
        {
            payment = new PaymentModel();
            billTo = new BillToModel();
        }

        public string customerType { get; set; }
        public BillToModel billTo { get; set; }
        public PaymentModel payment { get; set; }
        //public bool DefaultPaymentProfile { get; set; }

    }
}
