using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product
{
    public class ProductDto
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("assetAccount")]
        public EntityLinkRefObject AssetAccount { get; set; }

        [JsonProperty("atpMethod")]
        public EntityRefObject AtpMethod { get; set; }

        [JsonProperty("autoLeadTime")]
        public bool AutoLeadTime { get; set; }

        [JsonProperty("autoPreferredStockLevel")]
        public bool AutoPreferredStockLevel { get; set; }

        [JsonProperty("autoReorderPoint")]
        public bool AutoReorderPoint { get; set; }

        [JsonProperty("binNumber")]
        public EnityLinkRelObject binNumber { get; set; }

        [JsonProperty("class")]
        public EntityLinkRefObject Class { get; set; }

        [JsonProperty("cogsAccount")]
        public EntityLinkRefObject CogsAccount { get; set; }

        [JsonProperty("copyDescription")]
        public bool CopyDescription { get; set; }


        [JsonProperty("correlatedItems")]
        public EnityLinkRelObject CorrelatedItems { get; set; }

        [JsonProperty("cost")]
        public decimal Cost { get; set; }

        [JsonProperty("costCategory")]
        public EntityLinkRefObject CostCategory { get; set; }

        [JsonProperty("costEstimateType")]
        public EntityRefObject costEstimateType { get; set; }

        [JsonProperty("costingMethod")]
        public EntityRefObject CostingMethod { get; set; }

        [JsonProperty("createdDate")]
        public DateTime CreatedDate { get; set; }

        [JsonProperty("custitem13")]
        public EntityLinkRefObject Custitem13 { get; set; }

        [JsonProperty("custitem14")]
        public bool Custitem14 { get; set; }

        [JsonProperty("custitem15")]
        public bool Custitem15 { get; set; }

        [JsonProperty("custitem16")]
        public bool Custitem16 { get; set; }

        [JsonProperty("custitem17")]
        public bool Custitem17 { get; set; }

        [JsonProperty("custitem18")]
        public bool Custitem18 { get; set; }

        [JsonProperty("custitem19")]
        public bool Custitem19 { get; set; }

        [JsonProperty("custitem20")]
        public EntityLinkRefObject SpecificationClass { get; set; }

        [JsonProperty("custitem21")]
        public bool Custitem21 { get; set; }

        [JsonProperty("custitem22")]
        public bool Custitem22 { get; set; }

        [JsonProperty("custitem_price_list_description")]
        public string Custitem_price_list_description { get; set; }

        [JsonProperty("custitem_price_list_display_name")]
        public string Custitem_price_list_display_name { get; set; }

        [JsonProperty("customForm")]
        public EntityRefObject CustomForm { get; set; }

        [JsonProperty("enforceminqtyinternally")]
        public bool Enforceminqtyinternally { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("includeChildren")]
        public bool IncludeChildren { get; set; }

        [JsonProperty("incomeAccount")]
        public EntityLinkRefObject IncomeAccount { get; set; }

        [JsonProperty("isDropShipItem")]
        public bool IsDropShipItem { get; set; }

        [JsonProperty("isGCoCompliant")]
        public bool IsGCoCompliant { get; set; }

        [JsonProperty("isInactive")]
        public bool IsInactive { get; set; }

        [JsonProperty("isLotItem")]
        public bool IsLotItem { get; set; }

        [JsonProperty("isOnline")]
        public bool IsOnline { get; set; }

        [JsonProperty("isSerialItem")]
        public bool IsSerialItem { get; set; }

        [JsonProperty("isSpecialOrderItem")]
        public bool IsSpecialOrderItem { get; set; }

        [JsonProperty("itemId")]
        public string ItemId { get; set; }

        [JsonProperty("itemType")]
        public EntityRefObject ItemType { get; set; }

        [JsonProperty("itemVendor")]
        public EnityLinkRelObject ItemVendor { get; set; }

        [JsonProperty("lastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }

        [JsonProperty("locations")]
        public EnityLinkRelObject Locations { get; set; }

        [JsonProperty("matchBillToReceipt")]
        public bool MatchBillToReceipt { get; set; }

        [JsonProperty("offerSupport")]
        public bool OfferSupport { get; set; }

        [JsonProperty("price")]
        public EnityLinkRelObject Price { get; set; }

        [JsonProperty("purchaseDescription")]
        public string PurchaseDescription { get; set; }

        [JsonProperty("roundUpAsComponent")]
        public bool RoundUpAsComponent { get; set; }

        [JsonProperty("salesDescription")]
        public string SalesDescription { get; set; }

        [JsonProperty("seasonalDemand")]
        public bool SeasonalDemand { get; set; }

        [JsonProperty("shipIndividually")]
        public bool ShipIndividually { get; set; }

        [JsonProperty("subsidiary")]
        public EnityLinkRelObject Subsidiary { get; set; }

        [JsonProperty("supplyReplenishmentMethod")]
        public EntityRefObject SupplyReplenishmentMethod { get; set; }

        [JsonProperty("trackLandedCost")]
        public bool TrackLandedCost { get; set; }

        [JsonProperty("totalValue")]
        public decimal TotalValue { get; set; }

        [JsonProperty("useBins")]
        public bool UseBins { get; set; }

        [JsonProperty("useMarginalRates")]
        public bool UseMarginalRates { get; set; }

        


      

        [JsonProperty("weightUnit")]
        public EntityRefObject WeightUnit { get; set; }

        [JsonProperty("pricingGroup")]
        public EntityLinkRefObject PricingGroup { get; set; }

        [JsonProperty("custitem_item_collection")]
        public EntityLinkRefObject custitem_pricing_group { get; set; }

        [JsonProperty("custitem26")]
        public EnityLinkRelObject CategorySite { get; set; }
        // public ItemInventoryObject CategorySite { get; set; }

        [JsonProperty("weight")]
        public decimal Weight { get; set; }

        [JsonProperty("custitem_length")]
        public string ProductLength { get; set; }

        [JsonProperty("custitem27")]
        public string ProductWidth { get; set; }

        [JsonProperty("custitem28")]
        public string ProductHeight { get; set; }

        [JsonProperty("custitem29")]
        public decimal LoadCapacity { get; set; }

        [JsonProperty("custitem30")]
        public decimal ShipLength { get; set; }

        [JsonProperty("custitem31")]
        public decimal ShipWidth { get; set; }

        [JsonProperty("custitem32")]
        public decimal ShipHeight { get; set; }

        [JsonProperty("storeDetailedDescription")]
        public string storeDetailedDescription { get; set; }

        [JsonProperty("storeDescription")]
        public string storeDescription { get; set; }

        [JsonProperty("custitem23")]
        public bool UnShippable { get; set; }

        [JsonProperty("custitem_web_sold_out")]
        public bool custitem_web_sold_out { get; set; }

        [JsonProperty("custitem_branded_variants")]
        public EnityLinkRelObject custitem_branded_variants { get; set; }
    }

    public class EnityLinkRelObjectCategory
    {
        [JsonProperty("links")]
        public LinkRelObjectCat[] Links { get; set; }
    }

    public class LinkRelObjectCat
    {

        [JsonProperty("links")]
        public List<LinkRelObject> Links { get; set; }

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
}
