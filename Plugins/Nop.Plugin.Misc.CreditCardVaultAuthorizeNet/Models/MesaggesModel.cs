using System.Collections.Generic;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models
{
    public class MesaggesModel
    {
        public MesaggesModel()
        {
            Messages = new List<MessageModel>();
        }

        public string ResultCode { get; set; }
        public List<MessageModel> Messages { get; set; }
    }
}
