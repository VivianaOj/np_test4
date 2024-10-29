using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;

namespace Nop.Services.Invoices
{
    public partial interface IInvoiceService
    {
        IList<Invoice> GetInvoicesByCustomerId(int customerId);
        Invoice GetInvoicesByCustomerOrderId(int customerId, int saleOrderId);
        IList<Invoice> GetInvoicesByCompanyId(int CompanyId);
        Invoice GetInvoicesByTransId(int customerId, string transId);
        void UpdateOrder(Invoice invoice);
        void InsertInvoice(Invoice invoice);
        Invoice GetInvoiceById(int orderId);
        IList<Invoice> GetOrdersByIds(int[] orderIds);
        IPagedList<InvoiceTransaction> SearchInvoice(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false);

        void InsertInvoiceTransaction(InvoiceTransaction invoice);
        IList<InvoiceTransaction> GetTransactionByIds(int[] orderIds);

        InvoiceTransaction GetTransactionById(int id);
    }
}
