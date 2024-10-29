using Nop.Core;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.NNBoxSelector.Domain
{
    public class BSItemPack : BaseEntity
    {
        public Product Product { get; set; }
        public int Qty { get; set; }

    }
}
