using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;
using Nop.Core.Domain.NN;

namespace Nop.Services.NN
{
    public partial interface ILogImportNetsuiteService
    {
        void InsertLog(LogNetsuiteImport FreigthQuote);
        IList<LogNetsuiteImport> GetLogs();

		IPagedList<LogNetsuiteImport> SearchLogs(int storeId = 0,
			int vendorId = 0, int customerId = 0,
			int productId = 0, int affiliateId = 0, int warehouseId = 0,
			int billingCountryId = 0, string paymentMethodSystemName = null,
			DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
			List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
			string billingPhone = null, string billingEmail = null, string billingLastName = "",
			string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

	}
}
