using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.CustomFieldRef
{
    public class CustomFieldList
    {
        public List<CustomFieldRefDto> CustomFieldRefDto { get; set; }
    }

    public class CustomFieldRefDto
    {
        public string internalId { get; set; }
        public string scriptId { get; set; }
    }
}
