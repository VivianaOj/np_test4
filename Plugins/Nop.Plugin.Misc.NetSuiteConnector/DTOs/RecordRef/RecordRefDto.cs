using Nop.Plugin.Misc.NetSuiteConnector.Enum;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef
{
    public class RecordRefDto
    {
        public int externalId { get; set; }
        public int internalId { get; set; }
        public RecordType type { get; set; }
    }
}
