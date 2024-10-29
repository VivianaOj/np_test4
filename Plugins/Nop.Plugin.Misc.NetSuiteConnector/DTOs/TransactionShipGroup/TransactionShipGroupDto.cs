using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.TransactionShipGroup
{

    public class SalesOrderShipGroupList
    {
        public List<TransactionShipGroupDto> TransactionShipGroupDto { get; set; }
    }

    public class TransactionShipGroupDto
    {
        public string destinationAddress { get; set; }
        public RecordRefDto destinationAddressRef { get; set; }
        public double handlingRate { get; set; }
        public double handlingTax2Amt { get; set; }
        public string handlingTax2Rate { get; set; }
        public double handlingTaxAmt { get; set; }
        public RecordRefDto handlingTaxCode { get; set; }
        public string handlingTaxRate { get; set; }
        public long id { get; set; }
        public bool isFulfilled { get; set; }
        public bool isHandlingTaxable { get; set; }
        public bool isShippingTaxable { get; set; }
        public string shippingMethod { get; set; }
        public RecordRefDto shippingMethodRef { get; set; }
        public double shippingRate { get; set; }
        public double shippingTax2Amt { get; set; }
        public string shippingTax2Rate { get; set; }
        public double shippingTaxAmt { get; set; }
        public RecordRefDto shippingTaxCode { get; set; }
        public string shippingTaxRate { get; set; }
        public string sourceAddress { get; set; }
        public RecordRefDto sourceAddressRef { get; set; }
        public double weight { get; set; }

    }
}
