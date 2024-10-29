using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class BSItemPack : BaseEntity
    {
        public int Qty { get; set; }
        public int BsItemPackBoxId { get; set; }
        public int ProductId { get; set; }
        public virtual BSItemPackedBox BsItemPackBox { get; set; }
        public virtual Product Product { get; set; }

    }
}
