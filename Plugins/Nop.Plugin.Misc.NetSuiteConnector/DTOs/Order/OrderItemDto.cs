using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order
{
    // OrderItem
    public class OrderItemDto
    {
        public List<object> links { get; set; }
        public double amount { get; set; }
        public CommitInventory commitInventory { get; set; }
        public bool commitmentFirm { get; set; }
        public double costEstimate { get; set; }
        public double costEstimateRate { get; set; }
        public CostEstimateType costEstimateType { get; set; }
        public bool createWo { get; set; }
        public bool custcol3 { get; set; }
        public double custcol4 { get; set; }
        public string custcol5 { get; set; }
        public bool custcol_2663_isperson { get; set; }
        public double custcoltemp { get; set; }
        public string description { get; set; }
        public bool excludeFromPredictiveRisk { get; set; }
        public bool excludeFromRateRequest { get; set; }
        public InventoryDetail inventoryDetail { get; set; }
        public bool isClosed { get; set; }
        public bool isOpen { get; set; }
        public ProductItem item { get; set; }
        //public string itemType { get; set; }
        public int line { get; set; }
        public bool linked { get; set; }
        public bool marginal { get; set; }
        public double poRate { get; set; }
        public Price price { get; set; }
        public bool printItems { get; set; }
        public double quantity { get; set; }
        public double quantityAvailable { get; set; }
        public double quantityBackOrdered { get; set; }
        public double quantityBilled { get; set; }
        public double quantityCommitted { get; set; }
        public double quantityFulfilled { get; set; }
        public double quantityOnHand { get; set; }
        public double rate { get; set; }
        public string rateSchedule { get; set; }
    }


    public class CommitInventory
    {
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class CostEstimateType
    {
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class InventoryDetail
    {
        public List<object> links { get; set; }
    }

    public class ProductItem
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class Price
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

}
