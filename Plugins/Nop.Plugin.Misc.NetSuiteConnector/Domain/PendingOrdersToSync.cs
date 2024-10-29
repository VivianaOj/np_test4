using Nop.Core;
using System;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class PendingOrdersToSync : BaseEntity
    {
        public int OrderId { get; set; }

        public DateTime? FailedDate { get; set; }

        public DateTime? SuccessDate { get; set; }

        public bool Synchronized { get; set; }
    }
}
