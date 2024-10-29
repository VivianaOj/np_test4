using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;
using Nop.Core.Domain.NN;

namespace Nop.Services.NN
{
    public partial interface IPendingDataToSyncService
    {
        void InsertPendingDataToSync(PendingDataToSync pendingDataToSync);
        void UpdatetPendingDataToSync(PendingDataToSync pendingDataToSync);

        PendingDataToSync GetPendingDataToSync(int  id);

		List<PendingDataToSync> GetActivePendingDataToSync();

        List<PendingDataToSync> GetAllPendingDataToSync();

        PendingDataToSync GetPendingInvocesDataToSync(int id, ImporterIdentifierType type);

    }
}
