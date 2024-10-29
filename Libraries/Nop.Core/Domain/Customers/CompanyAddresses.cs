using Nop.Core;
using Nop.Core.Domain.Common;

namespace Nop.Core.Domain.Customers
{
    public partial class CompanyAddresses : BaseEntity
    {
        public int CompanyId { get; set; }
        public int AddressId { get; set; }
        public bool IsBilling { get; set; }
        public bool IsShipping { get; set; }
        public bool DeliveryRoute { get; set; }
        public string Label { get; set; }
        public string DeliveryRouteName { get; set; }
        public string DeliveryRouteId { get; set; }
        public virtual Company Company { get; set; }
        public virtual Address Address { get; set; }
    }
}
