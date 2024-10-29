using Newtonsoft.Json;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product
{
    public class EntityRefObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("refName")]
        public string RefName { get; set; }
    }
}