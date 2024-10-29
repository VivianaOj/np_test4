using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.NetSuiteConnector.Models.Configuration
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.Enabled")]
        public bool Enabled { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.AccountId")]
        public string AccountId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.ConsumerKey")]
        public string ConsumerKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.ConsumerSecret")]
        public string ConsumerSecret { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.TokenId")]
        public string TokenId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.TokenSecret")]
        public string TokenSecret { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.CompanyUrl")]
        public string CompanyUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.RestServicesUrl")]
        public string RestServicesUrl { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.AccessKeyID")]
        public string AccessKeyID { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.SecretAccessKey")]
        public string SecretAccessKey { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.S3Region")]
        public string S3Region { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.DefaultS3Folder")]
        public string DefaultS3Folder { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.ImagesTempFolder")]
        public string ImagesTempFolder { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NetSuiteConnector.Fields.ImageTypes")]
        public string ImageTypes { get; set; }
    }
}
