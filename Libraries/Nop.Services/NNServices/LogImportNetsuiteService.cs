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
    public partial class LogImportNetsuiteService : ILogImportNetsuiteService
    {
        #region Fields
        private readonly IRepository<LogNetsuiteImport> _logNetsuiteImportRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        public LogImportNetsuiteService(IEventPublisher eventPublisher, IRepository<LogNetsuiteImport> logNetsuiteImportRepository)
        {
            _logNetsuiteImportRepository = logNetsuiteImportRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertLog(LogNetsuiteImport freigthQuote)
        {
            if (freigthQuote == null)
                throw new ArgumentNullException(nameof(freigthQuote));

            _logNetsuiteImportRepository.Insert(freigthQuote);

            //event notification
            _eventPublisher.EntityInserted(freigthQuote);
        }



        public virtual  IList<LogNetsuiteImport> GetLogs()
        {
            var query = from o in _logNetsuiteImportRepository.Table
                        select o;

            return query.ToList();
        }


        public virtual IPagedList<LogNetsuiteImport> SearchLogs(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _logNetsuiteImportRepository.Table;

            List<IPagedList<LogNetsuiteImport>> addLogsGroup = new List<IPagedList<LogNetsuiteImport>>();

            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.DateCreate);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.DateCreate);

			var logs = query.Where(log => log.LastExecutionDateGeneral != null)
	                 .GroupBy(log => log.LastExecutionDateGeneral)
	                 .Select(group => new 
	                 {
		                 LastExecutionDate = group.Key,
		                 Logs = group.ToList()
	                 })
	                 .ToList();

			query = query.OrderByDescending(r=>r.LastExecutionDateGeneral).GroupBy(r => r.LastExecutionDateGeneral)
             .SelectMany(group => group.OrderByDescending(X => X.LastExecutionDateGeneral));
            //addLogsGroup.Add(
            //database layer paging
            return new PagedList<LogNetsuiteImport>(query, pageIndex, pageSize, getOnlyTotalCount);
        }


   
        #endregion



    }


}
