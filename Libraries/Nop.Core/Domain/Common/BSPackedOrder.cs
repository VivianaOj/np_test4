using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class BSPackedOrder : BaseEntity
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public virtual Order Order { get; set; }
        public virtual Customer Customer { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }
        public bool Active { get; set; }
        public string ShippingOptions { get; set; }
        public bool IsCommercial { get; set; }
    }
}
