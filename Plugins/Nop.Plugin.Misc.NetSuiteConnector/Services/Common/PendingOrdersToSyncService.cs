using Nop.Core.Data;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public class PendingOrdersToSyncService : IPendingOrdersToSyncService
    {
        #region Fields

        private readonly IRepository<PendingOrdersToSync> _repositoryPendingOrdersToSync;

        #endregion

        #region Ctor

        public PendingOrdersToSyncService(IRepository<PendingOrdersToSync> repositoryPendingOrdersToSync)
        {
            _repositoryPendingOrdersToSync = repositoryPendingOrdersToSync;
        }

        #endregion

        #region Methods

        public List<PendingOrdersToSync> GetAllPendingOrders()
        {
            return _repositoryPendingOrdersToSync.Table.Where(o => !o.Synchronized).ToList();
        }

        public void InsertOrUpdatePendingOrder(PendingOrdersToSync pendingOrdersToSync)
        {
            if (pendingOrdersToSync == null)
                throw new ArgumentException(nameof(pendingOrdersToSync));

            var pendingOrdersToSyncFound = _repositoryPendingOrdersToSync.Table.FirstOrDefault(p => p.OrderId == pendingOrdersToSync.OrderId);

            if (pendingOrdersToSyncFound == null)
                _repositoryPendingOrdersToSync.Insert(pendingOrdersToSync);
            else
            {
                pendingOrdersToSyncFound.FailedDate = pendingOrdersToSync.FailedDate;
                pendingOrdersToSyncFound.SuccessDate = pendingOrdersToSync.SuccessDate;
                pendingOrdersToSyncFound.Synchronized = pendingOrdersToSync.Synchronized;

                _repositoryPendingOrdersToSync.Update(pendingOrdersToSyncFound);
            }
        }

        #endregion
    }
}
