using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs
{
    public class RecordRefObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("refName")]
        public string ReferenceName { get; set; }
    }
}
