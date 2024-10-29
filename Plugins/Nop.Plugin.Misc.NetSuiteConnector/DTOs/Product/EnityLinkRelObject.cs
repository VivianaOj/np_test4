using Newtonsoft.Json;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product
{
    public class EnityLinkRelObject
    {
        [JsonProperty("links")]
        public LinkRelObject[] Links { get; set; }
    }

    public class LinkRelObject
    {
        [JsonProperty("rel")]
        public string Rel { get; set; }

        [JsonProperty("href")]
        public string Href { get; set; }
    }
}