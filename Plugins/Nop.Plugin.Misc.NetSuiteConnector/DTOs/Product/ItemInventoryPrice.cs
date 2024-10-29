using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product
{
    public class ItemInventoryPriceList
    {
        [JsonProperty("links")]
        public EnityLinkRelObject[] Links { get; set; }

        [JsonProperty("items")]
        public EnityLinkRelObject[] Items { get; set; }


    }
    public class ItemInventoryPrice
    {
        [JsonProperty("links")]
        public EnityLinkRelObject[] Links { get; set; }

        [JsonProperty("price")]
        public decimal Price { get; set; }

        public decimal discountDisplay { get; set; }

        [JsonProperty("priceLevel")]
        public EntityLinkRefObject PriceLevel { get; set; }

        [JsonProperty("priceLevelName")]
        public string PriceLevelName { get; set; }

        [JsonProperty("quantity")]
        public Quantity Quantity { get; set; }

        public decimal RetailWebStorePrice { get; set; }

    }

    public class Quantity
    {
        [JsonProperty("value")]
        public string Value { get; set; }
    }

}
