using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs
{
    public class DefaultObject
    {

        [JsonProperty("links")]
        public List<DefaultLinkObject> Link  { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("refName")]
        public string RefName { get; set; }
    }
}
