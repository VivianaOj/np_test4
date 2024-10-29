using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class AddresBookAdreesObject
    {
        #region Fields

        [JsonProperty("addr1")]
        public string Addr1 { get; set; }

        [JsonProperty("addr2")]
        public string Addr2 { get; set; }

        [JsonProperty("addressee")]
        public string Addressee { get; set; }

        [JsonProperty("addrText")]
        public string AddrText { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("custrecord144")]
        public List<int> LimitedAcces { get; set; }

        [JsonProperty("override")]
        public bool Override { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        #endregion

        #region RelationedFields

        [JsonProperty("country")]
        public RecordRefObject Country { get; set; }

        #endregion





    }
}
