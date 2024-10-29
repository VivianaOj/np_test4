using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Shipping.NNDelivery.Models
{
    public class DeliveryRoutesSearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.Location")]
        public string Location { get; set; }
    }
}
