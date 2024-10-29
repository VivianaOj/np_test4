using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.NNDelivery.Models
{
    public class DeliveryRoutesModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.Name")]
        public string Name { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.Location")]
        public string Location { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.ValueToSend")]
        public string ValueToSend { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.IdValueToSend")]
        public int IdValueToSend { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.Minimum")]
        public decimal Minimum { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.Fields.Available")]
        public bool Available { get; set; }
    }
}
