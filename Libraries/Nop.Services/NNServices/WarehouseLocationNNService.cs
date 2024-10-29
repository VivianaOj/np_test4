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
    public partial class WarehouseLocationNNService : IWarehouseLocationNNService
    {
        #region Fields
        private readonly IRepository<WarehouseLocationNN> _warehouseLocationRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        public WarehouseLocationNNService(IEventPublisher eventPublisher, IRepository<WarehouseLocationNN> warehouseLocationRepository)
        {
            _warehouseLocationRepository = warehouseLocationRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void CreateWarehouse(WarehouseLocationNN freigthQuote)
        {
            if (freigthQuote == null)
                throw new ArgumentNullException(nameof(freigthQuote));

            _warehouseLocationRepository.Insert(freigthQuote);

            //event notification
            _eventPublisher.EntityInserted(freigthQuote);
        }



        public virtual  IList<WarehouseLocationNN> GetWarehouseLocation()
        {
            var query = from o in _warehouseLocationRepository.Table
                        select o;

            return query.ToList();
        }

        
        public virtual WarehouseLocationNN GetWarehouseLocationId(int stateId)
        {
            var query = from o in _warehouseLocationRepository.Table
                        where o.Id == stateId
                        select o;

            return query.FirstOrDefault();
        }


        public virtual IPagedList<WarehouseLocationNN> SearchWarehouseLocation(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _warehouseLocationRepository.Table;


            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.UpdateDate);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.UpdateDate);
           

            //database layer paging
            return new PagedList<WarehouseLocationNN>(query, pageIndex, pageSize, getOnlyTotalCount);
        }


        public virtual void UpdateWarehouseLocation(WarehouseLocationNN warehouse)
        {
            if (warehouse == null)
                throw new ArgumentNullException(nameof(warehouse));

            _warehouseLocationRepository.Update(warehouse);

            //event notification
            _eventPublisher.EntityUpdated(warehouse);
        }

        #endregion



    }


}
