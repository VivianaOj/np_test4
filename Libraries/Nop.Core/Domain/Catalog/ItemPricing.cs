using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Catalog
{
    public partial class ItemPricing : BaseEntity
    {
        public int ProductId { get; set; }
        public int CompanyId { get; set; }
        public decimal Price { get; set; }

        public int PriceLevel { get; set; }
        public virtual Company Company { get; set; }
    }
}
