using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            Countries = new List<SelectListItem>();
            CustomerTypes = new List<SelectListItem>();
            ValidationModes = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.CreditCardVaultAuthorizeNet.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Plugins.Misc.CreditCardVaultAuthorizeNet.TransactionKey")]
        public string TransactionKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.CreditCardVaultAuthorizeNet.LoginId")]
        public string LoginId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType")]
        public string CustomerType { get; set; }

        [NopResourceDisplayName("Plugins.Misc.CreditCardVaultAuthorizeNet.ValidationMode")]
        public string ValidationMode { get; set; }

        [NopResourceDisplayName("Plugins.Misc.CreditCardVaultAuthorizeNet.DefaultCountry")]
        public string DefaultCountry { get; set; }

        public IList<SelectListItem> Countries { get; set; }

        public IList<SelectListItem> CustomerTypes { get; set; }

        public IList<SelectListItem> ValidationModes { get; set; }
    }
}