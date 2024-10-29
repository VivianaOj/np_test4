using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Shipping.NNDelivery.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            AddDeliveryRoutesModel = new DeliveryRoutesModel();
            DeliveryRoutesSearchModel = new DeliveryRoutesSearchModel();
        }

        [NopResourceDisplayName("Nop.Plugin.Shipping.NNDelivery.FixedRate")]
        public decimal FixedRate { get; set; }

        public DeliveryRoutesModel AddDeliveryRoutesModel { get; set; }

        public DeliveryRoutesSearchModel DeliveryRoutesSearchModel { get; set; }
    }
}
