using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Partners
{
    public class SalesOrderPartnersList
    {
        public List<PartnersDto> PartnersDto { get; set; }
    }

    public class PartnersDto
    {
        public double contribution { get; set; }
        public bool isPrimary { get; set; }
        public RecordRefDto partner { get; set; }
        public RecordRefDto partnerRole { get; set; }

    }
}
