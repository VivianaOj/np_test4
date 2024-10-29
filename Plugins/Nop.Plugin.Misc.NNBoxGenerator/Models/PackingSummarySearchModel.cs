using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NNBoxGenerator.Models
{
    public class PackingSummarySearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.OrderId")]
        public int OrderId { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.CustomerName")]
        public string CustomerName { get; set; }
    }
}
