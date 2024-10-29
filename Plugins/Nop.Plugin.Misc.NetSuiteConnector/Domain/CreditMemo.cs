using Nop.Core;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class CreditMemo : BaseEntity
    {
        public int CustomerId { get; set; }

        public decimal DiscountTotal { get; set; }

        public string Memo { get; set; }

        public string Message { get; set; }

        public int SalesOrder { get; set; }
    }
}
