using Nop.Core;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class Payment : BaseEntity
    {
        public string Type { get; set; }

        public string Name { get; set; }

        public decimal Total { get; set; }

        public int CustomerId { get; set; }

        public string TransactionId { get; set; }

        public int Approval { get; set; }

        public string AuthorizeId { get; set; }

        public int OrderSale { get; set; }

        public int Invoice { get; set; }
    }
}
