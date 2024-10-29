using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product
{
    public class EntityLinkRefObject
    {
        [JsonProperty("links")]
        public List<EnityLinkRelObject> Links { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("refName")]
        public string RefName { get; set; }
    }

    public class EntityLinkRefObjectList
    {
        [JsonProperty("links")]
        public List<EntityLinkRefObject> Links { get; set; }

        [JsonProperty("items")]
        public List<EntityLinkRefObject> items { get; set; }

    }

}
