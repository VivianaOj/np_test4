using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.NNBoxSelector.Domain
{
    public class BSBox : BaseEntity
    {
        public string Name { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal Weigth { get; set; }
        public decimal Height { get; set; }
        public decimal VolumenBox { get; set; }
        public bool Active { get; set; }


    }
}
