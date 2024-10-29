using Nop.Plugin.Misc.NetSuiteConnector.DTOs.CustomFieldRef;
using Nop.Plugin.Misc.NetSuiteConnector.Enum;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Address
{
    public class AddressDto
    {
        public string ddr1 { get; set; }
        public string addr2 { get; set; }
        public string addr3 { get; set; }
        public string addressee { get; set; }
        public string addrPhone { get; set; }
        public string addrText { get; set; }
        public string attention { get; set; }
        public string city { get; set; }
        public CountryEnum country { get; set; }
        public CustomFieldList customFieldList { get; set; }
        public string internalId { get; set; }
        //public boolean override { get; set; }
        public string state { get; set; }
        public string zip { get; set; }

    }
}
