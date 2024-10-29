using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models
{
    public class PaymentProfile
    {
        public PaymentProfile()
        {
            billTo = new BillToModel();
            payment = new PaymentModel();
        }

        public BillToModel billTo { get; set; }
        public PaymentModel payment { get; set; }
        public bool defaultPaymentProfile { get; set; }
    }
}
