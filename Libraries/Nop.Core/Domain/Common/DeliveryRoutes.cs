using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class DeliveryRoutes : BaseEntity
    {
        public string Location { get; set; }
        public string Name { get; set; }
        public decimal Minimum { get; set; }
        public bool Available { get; set; }

        public string ValueToSend { get; set; }
    }
}
