using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class BSBox  : BaseEntity
    {
        public string Name { get; set; }
        public decimal Length { get; set; }
        public decimal Width { get; set; }
        public decimal WeigthAvailable { get; set; }
        public decimal Height { get; set; }
        public decimal VolumenBox { get; set; }
        public bool Active { get; set; }

        public decimal WeigthBox { get; set; }

        
    }
  
}
