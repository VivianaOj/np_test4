using Newtonsoft.Json;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using Nop.Plugin.Misc.NetSuiteConnector.Models.Order;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order
{
    public class Link
    {
        public string rel { get; set; }
        public string href { get; set; }
    }

    public class AccountingBookDetail
    {
        public List<Link> links { get; set; }
    }

    public class BillAddressList
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class BillingAddress
    {
        public List<Link> links { get; set; }
    }

    public class Currency
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class CustomForm
    {
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class Entity
    {
        public List<Link> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class Item
    {
        public List<Link> links { get; set; }
    }

    public class Location
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class OrderStatus
    {
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class SalesRep
    {
        public List<Link> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class ShipAddressList
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class ShipMethod
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class ShippingAddress
    {
        public List<Link> links { get; set; }
    }

    public class Status
    {
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class Subsidiary
    {
        public List<Link> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class Terms
    {
        public List<object> links { get; set; }
        public string id { get; set; }
        public string refName { get; set; }
    }

    public class OrderDto
    {
        public List<Link> links { get; set; }
        public AccountingBookDetail accountingBookDetail { get; set; }
        public double altShippingCost { get; set; }
        public string billAddress { get; set; }
        public BillAddressList billAddressList { get; set; }
        public BillingAddress billingAddress { get; set; }
        public string billingAddress_text { get; set; }
        public bool canBeUnapproved { get; set; }
        public DateTime createdDate { get; set; }
        public Currency currency { get; set; }
        public CustomForm customForm { get; set; }
        public string email { get; set; }
        public Entity entity { get; set; }
        public double estGrossProfit { get; set; }
        public double estGrossProfitPercent { get; set; }
        public int exchangeRate { get; set; }
        public string id { get; set; }
        public Item item { get; set; }
        public DateTime lastModifiedDate { get; set; }
        public Location location { get; set; }
        public bool needsPick { get; set; }
        public OrderStatus orderStatus { get; set; }
        public string prevDate { get; set; }
        public int prevRep { get; set; }
        public string salesEffectiveDate { get; set; }
        public SalesRep salesRep { get; set; }
        public string shipAddress { get; set; }
        public ShipAddressList shipAddressList { get; set; }
        public bool shipComplete { get; set; }
        public string shipDate { get; set; }
        public bool shipIsResidential { get; set; }
        public ShipMethod shipMethod { get; set; }
        public bool shipOverride { get; set; }
        public ShippingAddress shippingAddress { get; set; }
        public string shippingAddress_text { get; set; }
        public double shippingCost { get; set; }
        public bool shippingCostOverridden { get; set; }
        public Status status { get; set; }
        public Subsidiary subsidiary { get; set; }
        public double subtotal { get; set; }
        public string suppressUserEventsAndEmails { get; set; }
        public Terms terms { get; set; }
        public bool toBeEmailed { get; set; }
        public bool toBeFaxed { get; set; }
        public bool toBePrinted { get; set; }
        public double total { get; set; }
        public double totalCostEstimate { get; set; }
        public string tranDate { get; set; }
        public string tranId { get; set; }
        public string updateDropshipOrderQty { get; set; }
        public string webStore { get; set; }
        public RecordRefDto paymentMethod { get; set; }

        public string linkedTrackingNumbers { get; set; }
        public Location custbody10 { get; set; }

        public Location custbody_delivery_routes_atl { get; set; }
        public Location custbody_delivery_routes_cinci { get; set; }
        public Location custbody_delivery_routes_nash { get; set; }

        public Location custbody_website_order_status { get; set; }
        public string custbody_website_order_number { get; set; }

        public string custbody34 { get; set; }

        public Location source { get; set; }

        public string custbody_taxjar_external_amount_n_n { get; set; }

        public decimal custbody_tax_mirror { get; set; }
        

    }

    public class OrderItemListDto
    {
        public List<Link> links { get; set; }
        public List<Item> items { get; set; }
        public int totalResults { get; set; }
    }

    public class TransactionDto
    {
        [JsonProperty("links")]
        public LinkRelObject[] Links { get; set; }

        [JsonProperty("company")]
        public string Company { get; set; }

        [JsonProperty("customerid")]
        public string Customerid { get; set; }

        [JsonProperty("date")]
        public string Date { get; set; }

        [JsonProperty("document")]
        public string Document { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("orderid")]
        public string Orderid { get; set; }

        [JsonProperty("tranId")]
        public string TranId { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("createddate")]
        public DateTime Createddate { get; set; }

        [JsonProperty("foreignamountunpaid")]
        public decimal Foreignamountunpaid { get; set; }

        [JsonProperty("foreigntotal")]
        public decimal Foreigntotal { get; set; }

        [JsonProperty("foreignamountpaid")]
        public decimal foreignamountpaid { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("lastmodifieddate")]
        public DateTime Lastmodifieddate { get; set; }

        [JsonProperty("otherrefnum")]
        public string otherrefnum { get; set; }

        [JsonProperty("custbody_website_order_number")]
        public string OrderNopId { get; set; }

        [JsonProperty("duedate")]
        public DateTime duedate { get; set; }

        [JsonProperty("unapplied")]
        public decimal? custbody_deposit_unapplied { get; set; }
    }

    public class TransactionUpdateDto
    {
        public string id { get; set; }
        public Location custbody_website_order_status { get; set; }
        public shipAddressList shipAddressList { get; set; }
        public Models.Order.ShippingAddress shippingAddress { get; set; }
        public shipMethod shipMethod { get; set; }
        public Location Location { get; set; }
        

    }
    public class TransactionListDto
    {
        [JsonProperty("links")]
        public List<Link> Links { get; set; }

        [JsonProperty("items")]
        public List<TransactionDto> Items { get; set; }

        [JsonProperty("count")]
        public int Count { get; set; }

        [JsonProperty("hasMore")]
        public bool HasMore { get; set; }

        [JsonProperty("offset")]
        public int Offset { get; set; }

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }
    }

    public class TransactionsInfoDto
    {
        public List<Link> links { get; set; }
        public List<TransactionLineListDto> items { get; set; }
        public int totalResults { get; set; }
    }
    public class TransactionLineListDto
    {
        [JsonProperty("links")]
        public List<Link> Links { get; set; }

        public string transaction { get; set; }
    }

    public class ItemFullfitmentDto
    {
        public Location createdFrom { get; set; }
        public string orderId { get; set; }
        public OrderStatus shipStatus { get; set; }
        public OrderStatus status { get; set; }
        public string tranId { get; set; }
        public string id { get; set; }
    }

    public class CompanyCustomers
    {
        public CompanyCustomers()
        {
            Customer = new List<Core.Domain.Customers.Customer>() ;
        }

        public CompanyCustomerMapping Company { get; set; }
       public List<Nop.Core.Domain.Customers.Customer> Customer { get; set; }
    }


    #region paytment Model
    public class PaytmentDto
    {
        public PaytmentDto()
        {
            apply = new ItemsDto();
            credit = new ItemsDto();
            payments = new ItemsDto();
            customer = new IdLink();
        }
        public ItemsDto apply { get; set; }
        public ItemsDto credit { get; set; }

        public ItemsDto payments { get; set; }
        public IdLink customer { get; set; }
        public decimal payment { get; set; }
        public int paymentoption { get; set; }
    }

    public class ItemsDto
    {
        public ItemsDto()
        {
            items = new List<ItemDto>();
        }
        public List<ItemDto> items { get; set; }      
    }

    public class ItemDto
    {
        public ItemDto()
		{
            doc = new IdLink();

        }
        public decimal amount { get; set; }
        public bool apply { get; set; }
        public int line { get; set; }

        public IdLink doc { get; set; }

    }

    public class IdLink
    {
        public int id { get; set; }
    }


    public class CreditMemo
    {
        public decimal unapplied { get; set; }
        public string tranId { get; set; }
        public decimal total { get; set; }
        public decimal subtotal { get; set; }
        public string id { get; set; }
    }
    #endregion

}
