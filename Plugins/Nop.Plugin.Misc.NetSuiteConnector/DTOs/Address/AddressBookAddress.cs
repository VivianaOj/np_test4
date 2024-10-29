using Newtonsoft.Json;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Address
{
    public class AddressBookAddress
    {
        //[JsonProperty("links")]
        //public DefaultLinkObject Links { get; set; }

        [JsonProperty("addr1")]
        public string Addr1 { get; set; }

        [JsonProperty("addr2")]
        public string Addr2 { get; set; }

        [JsonProperty("addressee")]
        public string Addressee { get; set; }

        [JsonProperty("addrPhone")]
        public string AddrPhone { get; set; }

        [JsonProperty("addrText")]
        public string AddrText { get; set; }

        [JsonProperty("attention")]
        public string attention { get; set; }

        [JsonProperty("city")]
        public string City { get; set; }

        [JsonProperty("country")]
        public EntityRefObject Country { get; set; }

        [JsonProperty("override")]
        public bool Override { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("zip")]
        public string Zip { get; set; }

        public bool custrecord_address_pending_approval { get; set; }

        [JsonProperty("custrecord_run_tracker_list")]
        public RecordRefObject APPROVEDROUTEDELIVERY { get; set; }


    }

    public class AddressDefine
	{
        public AddressBookAddress addressbookaddress { get; set; }
        public bool defaultBilling { get; set; }

        public bool defaultShipping { get; set; }

    }

    public class itemsList
    {
        [JsonProperty("items")]
        public List<AddressDefine> items { get; set; }
    }
    public class Addressbook
    {
        [JsonProperty("addressbook")]
        public itemsList  addressbook { get; set; }
    }

}
