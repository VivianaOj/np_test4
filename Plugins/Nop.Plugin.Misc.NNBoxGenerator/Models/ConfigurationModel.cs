using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NNBoxGenerator.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AddBoxModel = new BSBoxModel();
            BoxesSelectorSearchModel = new BoxesSelectorSearchModel();
            AvaliablePackingTypes = new List<SelectListItem>();
        }

        public BSBoxModel AddBoxModel { get; set; }

        public BoxesSelectorSearchModel BoxesSelectorSearchModel { get; set; }

        public NNBoxGeneratorSettings NNBoxSettings { get; set; }

        [NopResourceDisplayName("Plugins.Shipping.UPS.Fields.PackingType")]
        public int PackingType { get; set; }
        public IList<SelectListItem> AvaliablePackingTypes { get; set; }


    }

}
