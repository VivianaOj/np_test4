using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class AddressBookObject
    {
        #region Fields

        [JsonProperty("addressId")]
        public string AddressId { get; set; }

        [JsonProperty("addressBookAddress_text")]
        public string AddresBookAddress_Text { get; set; }

        [JsonProperty("defaultBilling")]
        public bool DefaultBilling { get; set; }

        [JsonProperty("defaultShipping")]
        public bool DefaultShipping { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("internalId")]
        public int InternalId { get; set; }

        [JsonProperty("isResidential")]
        public bool IsResidential { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        #endregion

        #region Relationed Fields

        [JsonProperty("addressBookAddress")]
        public AddresBookAdreesObject AddressBookAddress { get; set; }

        #endregion



    }
}
