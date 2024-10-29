using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class BSItemPackedBox : BaseEntity
    {
        public decimal PercentBoxVolumePacked { get; set; }
        public decimal PackTime { get; set; }
        public int ItemsPacked { get; set; }
        public decimal WeightTotalBox { get; set; }
        public decimal VolumenTotalBox { get; set; }
        public string GetShippingOptionResponse { get; set; }
        public string ContainerPackingResult { get; set; }
        public int CustomerId { get; set; }
        public int ContainerId { get; set; }
        public virtual Customer Customer { get; set; } 
        public  bool Active { get; set; }
        public int OrderId { get; set; }
        public string  ContainerName { get; set; }
        public virtual Order Order { get; set; }
        public decimal FinalHeight { get; set; }
        public decimal FinalWidth { get; set; }
        public decimal FinalLength { get; set; }
        public int BSPackedOrderId { get; set; }
        public bool IsAsShip { get; set; }
        
    }
}
