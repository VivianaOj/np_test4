using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class Subscriptions : BaseEntity  
    {
        public DateTime LastModifiedDate { get; set; }

        public bool Subscribed { get; set; }
        public int IdNetSuite { get; set; }

        public int SubscriptionName { get; set; }

    }
}
