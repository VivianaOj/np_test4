using Nop.Plugin.Misc.NetSuiteConnector.DTOs.InventoryAssignment;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.InventoryDetail
{
    public class InventoryDetailDto
    {
        public RecordRefDto customForm { get; set; }
        public InventoryAssignmentList inventoryAssignmentList { get; set; }
    }
}
