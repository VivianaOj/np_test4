
namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models
{
    public class ProfileModel
    {
        public ProfileModel()
        {
            paymentProfiles = new PaymentProfilesModel();
        }

        public string merchantCustomerId { get; set; }
        public string description { get; set; }
        public string email { get; set; }
        public PaymentProfilesModel paymentProfiles { get; set; }
       
 

    }
}
