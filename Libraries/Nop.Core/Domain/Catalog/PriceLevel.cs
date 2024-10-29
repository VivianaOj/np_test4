using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Catalog
{
    public partial class PriceLevel : BaseEntity
    {
        public string IdNetSuite { get; set; }
        public string Name { get; set; }
    }
}
