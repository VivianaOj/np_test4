using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Nop.Core.Domain.Shipping
{
    /// <summary>
    /// Represents a shipping option
    /// </summary>
    public partial class ShippingOption
    {
        /// <summary>
        /// Gets or sets the system name of shipping rate computation method
        /// </summary>
        public string ShippingRateComputationMethodSystemName { get; set; }

        /// <summary>
        /// Gets or sets a shipping rate (without discounts, additional shipping charges, etc)
        /// </summary>
        public decimal Rate { get; set; }

        /// <summary>
        /// Gets or sets a shipping option name
        /// </summary>
        public string Name { get; set; }

        public string ValueToSendOrder { get; set; }

        public int IdValueToSendOrder { get; set; }

        public string LocationName { get; set; }

        /// <summary>
        /// Gets or sets a shipping option description
        /// </summary>
        public string Description { get; set; }
        public  bool Disable  { get; set; }

        public bool IsCommercial { get; set; }
        public string Address { get; set; }
        public int AddressId { get; set; }

        public string PickupPersonNote { get; set; }

        public string NetsuiteLocationName { get; set; }

        public string NetsuiteId { get; set; }
        
    }

}
