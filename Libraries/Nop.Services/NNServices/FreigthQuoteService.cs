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
    public partial class FreigthQuoteService : IFreigthQuoteService
    {
        #region Fields
        private readonly IRepository<FreightQuote> _freigthQuoteRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        public FreigthQuoteService(IEventPublisher eventPublisher, IRepository<FreightQuote> freigthQuoteRepository)
        {
            _freigthQuoteRepository = freigthQuoteRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertFreightQuote(FreightQuote freigthQuote)
        {
            if (freigthQuote == null)
                throw new ArgumentNullException(nameof(freigthQuote));

            _freigthQuoteRepository.Insert(freigthQuote);

            //event notification
            _eventPublisher.EntityInserted(freigthQuote);
        }



        public virtual  IList<FreightQuote> GetFreigthQuote()
        {
            var query = from o in _freigthQuoteRepository.Table
                        select o;

            return query.ToList();
        }


        public virtual IPagedList<FreightQuote> SearchFreigthQuote(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _freigthQuoteRepository.Table;


            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.RequestDate);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.RequestDate);
           

            //database layer paging
            return new PagedList<FreightQuote>(query, pageIndex, pageSize, getOnlyTotalCount);
        }


   
        #endregion



    }


}
