using Nop.Core;

namespace Nop.Plugin.Shipping.NNDelivery.Domain
{
    public class DeliveryRoutes : BaseEntity
    {
        public string Name { get; set; }

        public string Location { get; set; }

       public string ValueToSend { get; set; }

        public int IdValueToSend { get; set; }

        public decimal Minimum { get; set; }

        public bool Available { get; set; }
    }
}