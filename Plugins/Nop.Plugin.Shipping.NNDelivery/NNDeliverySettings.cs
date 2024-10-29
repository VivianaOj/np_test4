using Nop.Core.Configuration;

namespace Nop.Plugin.Shipping.NNDelivery
{
    public class NNDeliverySettings : ISettings
    {
        public decimal FixedRate { get; set; }
    }
}