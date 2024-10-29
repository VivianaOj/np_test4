using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class CustomersObject
    {
        [JsonProperty("links")]
        public List<DefaultLinkObject> Links { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("items")]
        public List<LinksObject> Items { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
    }

    public class LinksObject
    {
        [JsonProperty("links")]
        public List<DefaultLinkObject> Links { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }
    }
}
