using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Common
{
    public partial interface IAnywhereSliderService
    {
        SS_AS_AnywhereSlider GetRegiusterById(string SystemName);
        SS_AS_SliderImage GetSliderById(int SliderId);
    }
}
