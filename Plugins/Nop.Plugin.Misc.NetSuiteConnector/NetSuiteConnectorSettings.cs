using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.NetSuiteConnector
{
    public class NetSuiteConnectorSettings : ISettings
    {
        #region Connection Settings

        public bool Enabled { get; set; }

        public string AccountId { get; set; }

        public string ConsumerKey { get; set; }

        public string ConsumerSecret { get; set; }

        public string TokenId { get; set; }

        public string TokenSecret { get; set; }

        public string CompanyUrl { get; set; }

        public string RestServicesUrl { get; set; }

        #endregion

        #region Media Settings

        public string AccessKeyID { get; set; }

        public string SecretAccessKey { get; set; }

        public string S3Region { get; set; }

        public string DefaultS3Folder { get; set; }

        public string ImagesTempFolder { get; set; }

        public string ImageTypes { get; set; }

        #endregion
    }
}
