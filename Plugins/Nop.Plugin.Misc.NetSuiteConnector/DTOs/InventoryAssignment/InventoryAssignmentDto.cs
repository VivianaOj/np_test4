using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.InventoryAssignment
{
    public class InventoryAssignmentList
    {
        public List<InventoryAssignmentDto> InventoryAssignment { get; set; }
    }

    public class InventoryAssignmentDto
    {
        public RecordRefDto binNumber { get; set; }
        public DateTime expirationDate { get; set; }
        public string internalId { get; set; }
        public RecordRefDto issueInventoryNumber { get; set; }
        public double quantity { get; set; }
        public double quantityAvailable { get; set; }
        public string receiptInventoryNumber { get; set; }
        public RecordRefDto toBinNumber { get; set; }

    }
}
