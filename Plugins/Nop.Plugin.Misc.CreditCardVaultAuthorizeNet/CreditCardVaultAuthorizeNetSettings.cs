using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet
{
    public class CreditCardVaultAuthorizeNetSettings : ISettings
    {
        public bool Enabled { get; set; }

        public string TransactionKey { get; set; }

        public string LoginId { get; set; }

        public string CustomerType { get; set; }

        public string ValidationMode { get; set; }

        public string DefaultCountry { get; set; }
    }
}
