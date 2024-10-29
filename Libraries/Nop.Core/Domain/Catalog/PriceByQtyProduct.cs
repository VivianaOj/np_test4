using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Catalog
{
    public partial class PriceByQtyProduct : BaseEntity
    {
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int PriceLevelId { get; set; }

        public int ProductId { get; set; }

        public virtual Product Product { get; set; }
    }
}
