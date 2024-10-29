using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs
{
    public class DefaultLinkObject
    {
        #region Fields

        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }

        #endregion

    }
}
