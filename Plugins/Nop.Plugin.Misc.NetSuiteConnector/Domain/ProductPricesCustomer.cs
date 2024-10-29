using Nop.Core;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class ProductPricesCustomer : BaseEntity
    {
        public int ProductId { get; set; }

        public int ItemInventoryId { get; set; }

        public int CustomerId { get; set; }

        public decimal Price { get; set; }

        public int DiscountId { get; set; }
    }
}
