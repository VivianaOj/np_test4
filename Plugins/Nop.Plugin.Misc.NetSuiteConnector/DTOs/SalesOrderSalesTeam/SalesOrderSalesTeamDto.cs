using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.SalesOrderSalesTeam
{
    public class SalesOrderSalesTeamList
    {
        public List<SalesOrderSalesTeamDto> SalesOrderSalesTeamDto { get; set; }
    }

    public class SalesOrderSalesTeamDto
    {
        public double contribution { get; set; }
        public RecordRefDto employee { get; set; }
        public bool isPrimary { get; set; }
        public RecordRefDto salesRole { get; set; }

    }
}
