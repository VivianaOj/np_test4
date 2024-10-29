using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Tax.TaxJar.Models
{
    public class ConfigurationModel
    {
        [NopResourceDisplayName("Plugins.Tax.TaxJar.Fields.TaxJarAPIKey")]
        public string TaxJarAPIKey { get; set; }
    }
}
