using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.NNBoxSelector.Domain
{
    public class BSItemPackedBox : BaseEntity
    {
        public decimal PercentBoxVolumePacked { get; set; }
        public decimal PackTime { get; set; }
        public int ItemsPacked { get; set; }
        public int WeightTotalBox { get; set; }
        public int VolumenTotalBox { get; set; }
    }
}
