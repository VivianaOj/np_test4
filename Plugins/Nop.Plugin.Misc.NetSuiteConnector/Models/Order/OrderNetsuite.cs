using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Nop.Plugin.Misc.NetSuiteConnector.Models.Order
{
    public class OrderNetsuite
    {
        public OrderNetsuite()
        {
            DynamicProperties = new ExpandoObject();
        }
        public string id { get; set; }
        public string entity { get; set; }
        public string otherRefNum { get; set; }

        public string custbody_webstore_email { get; set; }
        public Location customForm { get; set; }
        public Item item { get; set; }
        public Location Location { get; set; }
        public bool custbody58 { get; set; }
        public ShippingAddress shippingAddress { get; set; }
        public BillingAddress billingAddress { get; set; }
        public shipAddressList shipAddressList { get; set; }
        public custbody8 custbody8 { get; set; }
        public orderStatus orderStatus { get; set; }
        public shipMethod shipMethod { get; set; }
        public Location custbody_website_order_status { get; set; }
        public decimal shippingCost { get; set; }
        public string custbody_web_address { get; set; }
        public string custbody31 { get; set; }
        public string custbody_website_order_number { get; set; }
        //public Location custbody_delivery_routes_atl { get; set; }
        //public Location custbody_delivery_routes_cinci { get; set; }
        //public Location custbody_delivery_routes_nash { get; set; }

        public ExpandoObject DynamicProperties { get; set; }

       public decimal custbody_tj_external_tax_amount { get; set; }
        public decimal custbody_taxjar_external_amount_n_n { get; set; }
        public bool shipIsResidential { get; set; }
        public string custbody_promotion { get; set; }
        public string custbody_website_user_role { get; set; }
        public string custbody_pickup_person_notes { get; set; }
        
    }
    public class OrderUpdateNetsuite
    {
        public string id { get; set; }
        public Location Location { get; set; }
        public bool custbody58 { get; set; }
        //public ShippingAddress shippingAddress { get; set; }
        //public BillingAddress billingAddress { get; set; }
        //public shipAddressList shipAddressList { get; set; }
        public custbody8 custbody8 { get; set; }
        public shipMethod shipMethod { get; set; }
        public Location custbody_website_order_status { get; set; }
        public Location customForm { get; set; }
        //public shipAddressList billAddressList { get; set; }
        public decimal custbody_tj_external_tax_amount { get; set; }

        public decimal custbody_taxjar_external_amount_n_n { get; set; }

        public decimal shippingCost { get; set; }

        public string custbody_website_order_number { get; set; }
    }

    public class OrderUpdateNetsuiteOrderId
    {
        public string custbody_website_order_number { get; set; }
    }

    public class shipMethod
    {
        public string id { get; set; }
    }
    public class orderStatus
    {
        public string id { get; set; }
    }

    public class custbody8
    {
        public string id { get; set; }
    }

    public class shipAddressList
    {
        public string id { get; set; }
        public bool custrecord209 { get; set; }
    }

    public class Location
    {
        public string id { get; set; }

        public string refName { get; set; }
    }

    public class Item
    {
        public List<ItemDetail> items { get; set; }

    }
    public class ItemDetail
    {
        public InventoryItem item { get; set; }
        public decimal amount { get; set; }
        public int quantity { get; set; }

        public decimal rate { get; set; }
        public string custcol_item_attribute { get; set; }


    }
    public class InventoryItem
    {
        public string id { get; set; }
    }

    public class ShippingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string addr1 { get; set; }
        public string addr2 { get; set; }
        public string addrphone { get; set; }
        public string city { get; set; }
        public Location country { get; set; }
        public string state { get; set; }

        public string zip { get; set; }

        public string addressee { get; set; }

        public string attention { get; set; }

    }

    public class BillingAddress
    {
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string addr1 { get; set; }
        public string addr2 { get; set; }
        public string addrphone { get; set; }
        public string city { get; set; }
        public Location country { get; set; }
        public string state { get; set; }

        public string zip { get; set; }


    }
}
