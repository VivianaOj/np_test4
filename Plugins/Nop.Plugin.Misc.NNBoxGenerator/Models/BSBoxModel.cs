using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NNBoxGenerator.Models
{
    public class BSBoxModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.Length")]
        public decimal Length { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.Width")]
        public decimal Width { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.WeigthAvailable")]
        public decimal WeigthAvailable { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.WeigthBox")]
        public decimal WeigthBox { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.WeigthTotalBox")]
        public decimal WeigthTotalBox { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.Height")]
        public decimal Height { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.VolumenBox")]
        public decimal VolumenBox { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.Active")]
        public bool Active { get; set; }

    }
}
