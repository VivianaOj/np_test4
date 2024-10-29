using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;
using Nop.Core.Domain.NN;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.NN
{
    public partial class PendingDataToSyncService : IPendingDataToSyncService
    {
        #region Fields
        private readonly IRepository<PendingDataToSync> _pendingDataToSyncRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        public PendingDataToSyncService(IEventPublisher eventPublisher, IRepository<PendingDataToSync> pendingDataToSyncRepository)
        {
            _pendingDataToSyncRepository = pendingDataToSyncRepository;
            _eventPublisher = eventPublisher;
        }

		public PendingDataToSync GetPendingDataToSync(int id)        
        {
            var query = from o in _pendingDataToSyncRepository.Table
                        where o.IdItem==id
                        select o;

            return query.Take(100).FirstOrDefault();
        }


        public PendingDataToSync GetPendingInvocesDataToSync(int id, ImporterIdentifierType type)
        {
            var query = from o in _pendingDataToSyncRepository.Table
                        where o.IdItem == id && o.Type == (int)type
                        select o;

            return query.Take(100).FirstOrDefault();
        }

        public void UpdatetPendingDataToSync(PendingDataToSync pendingDataToSync)
        {
            if (pendingDataToSync == null)
                throw new ArgumentNullException(nameof(pendingDataToSync));

            _pendingDataToSyncRepository.Update(pendingDataToSync);

            //event notification
            _eventPublisher.EntityUpdated(pendingDataToSync);
        }

        public List<PendingDataToSync> GetActivePendingDataToSync()
        {
            var query = from o in _pendingDataToSyncRepository.Table
                        where o.Synchronized ==false
                        select o;

            return query.OrderByDescending(r=>r.Id).Take(100).ToList();
        }

        public List<PendingDataToSync> GetAllPendingDataToSync()
        {
            var query = from o in _pendingDataToSyncRepository.Table
                        select o;

            return query.OrderByDescending(r=>r.UpdateDate).Take(100).ToList();
        }
        

        #endregion

        #region Methods


        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertPendingDataToSync(PendingDataToSync pendingDataToSync)
        {
            if (pendingDataToSync == null)
                throw new ArgumentNullException(nameof(pendingDataToSync));

            _pendingDataToSyncRepository.Insert(pendingDataToSync);

            //event notification
            _eventPublisher.EntityInserted(pendingDataToSync);
        }




        #endregion



    }


}
