using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Responses
{
    public class ResponseGetCustomerPaymentProfileModel
    {
        public ResponseGetCustomerPaymentProfileModel()
        {
            PaymentProfile = new PaymentProfilesModel();
            BillTo = new BillToModel();
            Messages = new MesaggesModel();
        }

        public PaymentProfilesModel PaymentProfile { get; set; }
        public string OriginalNetworkTransId { get; set; }
        public int OriginalAuthAmount { get; set; }
        public BillToModel BillTo { get; set; }
        public MesaggesModel Messages { get; set; }
    }
}
