using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class ItemPricingObject
    {
        [JsonProperty("links")]
        public List<DefaultLinkObject> Links { get; set; }


        [JsonProperty("items")]
        public List<LinksObject> Items { get; set; }

        [JsonProperty("level")]
        public Level Livel { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        public class Level
        {
            [JsonProperty("links")]
            public List<DefaultLinkObject> Links { get; set; }

            [JsonProperty("id")]
            public string Id{ get; set; }

            [JsonProperty("refName")]
            public string ReferenceName { get; set; }
        }           
    }
}
