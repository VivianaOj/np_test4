using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.NN
{
    public partial class WarehouseLocationNN : BaseEntity
    {
        public DateTime UpdateDate { get; set; }

        public int StateProvideceId { get; set; }

        public int WarehouseId { get; set; }

        public int UserId { get; set; }

    }
}

