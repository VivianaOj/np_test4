using Newtonsoft.Json;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.AccountingBookDetail;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Address;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.CustomFieldRef;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.GiftCertRedemption;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Partners;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Promotions;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.SalesOrderItem;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.SalesOrderSalesTeam;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.TransactionShipGroup;
using Nop.Plugin.Misc.NetSuiteConnector.Enum;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order
{

    public class InvoiceDto
    {
        public List<Link> links { get; set; }
        //public AccountingBookDetail accountingBookDetail { get; set; }
        //public double amountPaid { get; set; }
        //public double amountRemaining { get; set; }
        //public double amountRemainingTotalBox { get; set; }
        //public BillAddressList billAddressList { get; set; }
        //public string billAddress { get; set; }
        //public BillingAddress billingAddress { get; set; }
        //public string billingAddress_text { get; set; }
        public DateTime createdDate { get; set; }
        public Entity createdFrom { get; set; }
        //public Currency currency { get; set; }
        //public Entity custbody10 { get; set; }

        //public Item custbody13 { get; set; }
        //public decimal custbody19 { get; set; }
        //public decimal custbody20 { get; set; }
        //public decimal custbody21 { get; set; }
        //public bool custbody22 { get; set; }
        //public bool custbody26 { get; set; }
        //public Item custbody3 { get; set; }

        //public Item custbody47 { get; set; }
        //public decimal custbody49 { get; set; }
        //public bool custbody52 { get; set; }
        //public string custbody53 { get; set; }
        //public string custbody54 { get; set; }
        //public bool custbody55 { get; set; }
        //public bool custbody57 { get; set; }
        //public bool custbody58 { get; set; }
        //public bool custbody59 { get; set; }
        //public decimal custbody6 { get; set; }
        //public string custbody7 { get; set; }
        //public Entity custbody_created_by { get; set; }
        //public DateTime custbody_esc_created_date { get; set; }
        //public DateTime custbody_esc_last_modified_date { get; set; }
        //public bool custbody_fault { get; set; }
        //public Item custbody_nn_dock { get; set; }
        //public CustomForm customForm { get; set; }
        //public DateTime dueDate { get; set; }
        //public string email { get; set; }
        public Entity entity { get; set; }
        //public decimal estGrossProfit { get; set; }
        //public decimal estGrossProfitPercent { get; set; }
        //public int exchangeRate { get; set; }
         public int id { get; set; }
        //public Item item { get; set; }
        public DateTime lastModifiedDate { get; set; }
        //public Entity location { get; set; }
        //public string otherRefNum { get; set; }
        //public Entity postingPeriod { get; set; }
        //public DateTime prevDate { get; set; }

        //public DateTime salesEffectiveDate { get; set; }
        //public string shipAddress { get; set; }
        //public Entity shipAddressList { get; set; }
        //public DateTime shipDate { get; set; }

        //public bool shipIsResidential { get; set; }
        //public bool shipOverride { get; set; }
        //public Item shippingAddress { get; set; }
        //public string shippingAddress_text { get; set; }

        //public bool shippingCostOverridden { get; set; }
        public Status status { get; set; }
        //public Entity subsidiary { get; set; }
        public decimal subtotal { get; set; }

        //public Entity terms { get; set; }

        //public bool toBeEmailed { get; set; }
        //public bool toBeFaxed { get; set; }

        //public bool toBePrinted { get; set; }

        public decimal total { get; set; }
        //public decimal totalCostEstimate { get; set; }
        //public DateTime tranDate { get; set; }
        public string tranId { get; set; }

        public decimal amountRemaining { get; set; }
    }
}