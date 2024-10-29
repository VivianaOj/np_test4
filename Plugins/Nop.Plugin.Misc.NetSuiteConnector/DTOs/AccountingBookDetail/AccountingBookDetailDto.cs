using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.AccountingBookDetail
{
    public class AccountingBookDetailDtoList
    {
        public List<AccountingBookDetailDto> accountingBook { get; set; }
    }

    public class AccountingBookDetailDto
    {
        public RecordRefDto accountingBook { get; set; }
        public RecordRefDto currency { get; set; }
        public double exchangeRate { get; set; }
    }
}
