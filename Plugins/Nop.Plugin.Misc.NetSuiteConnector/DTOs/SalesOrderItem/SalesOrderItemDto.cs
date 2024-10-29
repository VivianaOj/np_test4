using Nop.Plugin.Misc.NetSuiteConnector.DTOs.CustomFieldRef;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.InventoryDetail;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using Nop.Plugin.Misc.NetSuiteConnector.Enum;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.SalesOrderItem
{
    public class SalesOrderItemList
    {
        public List<SalesOrderItemDto> SalesOrderItemDto { get; set; }
    }

    public class SalesOrderItemDto
    {
        public double altSalesAmt { get; set; }
        public double amount { get; set; }
        public RecordRefDto BillingSchedule { get; set; }
        public RecordRefDto catchUpPeriod { get; set; }
        public RecordRefDto chargeType { get; set; }
        //public RecordRefDto class { get; set; }
        public SalesOrderItemCommitInventory commitInventory { get; set; }
        public double costEstimate { get; set; }
        public ItemCostEstimateType costEstimateType { get; set; }
        public RecordRefDto createdPo { get; set; }
        public SalesOrderItemCreatePo createPo { get; set; }
        public bool createWo { get; set; }
        public CustomFieldList customFieldList { get; set; }
        public bool deferRevRec { get; set; }
        public RecordRefDto department { get; set; }
        public string description { get; set; }
        public bool excludeFromRateRequest { get; set; }
        public bool expandItemGroup { get; set; }
        public DateTime expectedShipDate { get; set; }
        public bool fromJob { get; set; }
        public string giftCertFrom { get; set; }
        public string giftCertMessage { get; set; }
        public string giftCertNumber { get; set; }
        public string giftCertRecipientEmail { get; set; }
        public string giftCertRecipientName { get; set; }
        public double grossAmt { get; set; }
        public InventoryDetailDto inventoryDetail { get; set; }
        public bool isClosed { get; set; }
        public bool isEstimate { get; set; }
        public bool isTaxable { get; set; }
        public RecordRefDto item { get; set; }
        public bool itemIsFulfilled { get; set; }
        public RecordRefDto job { get; set; }
        public string licenseCode { get; set; }
        public long line { get; set; }
        public RecordRefDto location { get; set; }
        public bool locationAutoAssigned { get; set; }
        public bool noAutoAssignLocation { get; set; }
        public CustomFieldList options { get; set; }
        public double orderPriority { get; set; }
        public double percentComplete { get; set; }
        public string poCurrency { get; set; }
        public double poRate { get; set; }
        public RecordRefDto poVendor { get; set; }
        public RecordRefDto price { get; set; }
        public double quantity { get; set; }
        public double quantityAvailable { get; set; }
        public double quantityBackOrdered { get; set; }
        public double quantityBilled { get; set; }
        public double quantityCommitted { get; set; }
        public double quantityFulfilled { get; set; }
        public double quantityOnHand { get; set; }
        public double quantityPacked { get; set; }
        public double quantityPicked { get; set; }
        public string rate { get; set; }
        public DateTime revRecEndDate { get; set; }
        public RecordRefDto revRecSchedule { get; set; }
        public DateTime revRecStartDate { get; set; }
        public long revRecTermInMonths { get; set; }
        public string serialNumbers { get; set; }
        public RecordRefDto shipAddress { get; set; }
        public long shipGroup { get; set; }
        public RecordRefDto shipMethod { get; set; }
        public RecordRefDto subscription { get; set; }
        public double tax1Amt { get; set; }
        public RecordRefDto taxCode { get; set; }
        public double taxRate1 { get; set; }
        public double taxRate2 { get; set; }
        public RecordRefDto units { get; set; }
        public double vsoeAllocation { get; set; }
        public double vsoeAmount { get; set; }
        public VsoeDeferral vsoeDeferral { get; set; }
        public bool vsoeDelivered { get; set; }
        public bool vsoeIsEstimate { get; set; }
        public VsoePermitDiscount vsoePermitDiscount { get; set; }
        public double vsoePrice { get; set; }
        public VsoeSopGroup vsoeSopGroup { get; set; }

    }
}
