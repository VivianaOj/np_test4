using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public interface IPendingOrdersToSyncService
    {
        List<PendingOrdersToSync> GetAllPendingOrders();

        void InsertOrUpdatePendingOrder(PendingOrdersToSync pendingOrdersToSync);             
    }
}