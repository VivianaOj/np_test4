﻿using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.NNBoxSelector.Models
{
    public class BoxesSelectorSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.Name")]
        public string Name { get; set; }
    }
}
