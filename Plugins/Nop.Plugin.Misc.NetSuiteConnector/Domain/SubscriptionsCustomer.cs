using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class SubscriptionsCustomer : BaseEntity
    {
        public int CustomerId { get; set; }

        public bool SubscriptionId { get; set; }

        public bool IsActive { get; set; }
    }
}
