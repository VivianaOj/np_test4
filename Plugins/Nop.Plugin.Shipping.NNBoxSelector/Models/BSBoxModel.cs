using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.NNBoxSelector.Models
{
    public class BSBoxModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.Length")]
        public decimal Length { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.Width")]
        public decimal Width { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.Weigth")]
        public decimal Weigth { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.Height")]
        public decimal Height { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.VolumenBox")]
        public decimal VolumenBox { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNBoxSelector.Fields.Active")]
        public bool Active { get; set; }

    }
}
