using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class SS_AS_SliderImage : BaseEntity
    {
        public int Id { get; set; }
        public string DisplayText { get; set; }
        public string Url { get; set; }
        public string Alt { get; set; }
        public bool Visible { get; set; }
        public int DisplayOrder { get; set; }
        public int PictureId { get; set; }
        public int MobilePictureId { get; set; }
        public int SliderId { get; set; }
    }
}
