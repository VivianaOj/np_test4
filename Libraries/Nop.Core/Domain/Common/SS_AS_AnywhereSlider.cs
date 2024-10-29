using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class SS_AS_AnywhereSlider : BaseEntity
    {
        public int Id { get; set; }
        public string SystemName { get; set; }
        public int SliderType { get; set; }
        public int LanguageId { get; set; }
        public bool LimitedToStores { get; set; }
    }
}
