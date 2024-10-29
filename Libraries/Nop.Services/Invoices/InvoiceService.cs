using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Invoices
{
    public partial class InvoiceService : IInvoiceService
    {
        #region Fields
        private readonly IRepository<Invoice> _invoiceRepository;

        private readonly IRepository<InvoiceTransaction> _InvoiceTransaction;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        public InvoiceService(IEventPublisher eventPublisher, IRepository<InvoiceTransaction> InvoiceTransaction,IRepository<Invoice> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
            _eventPublisher = eventPublisher;
            _InvoiceTransaction = InvoiceTransaction;
        }

        #endregion

        #region Methods

        IList<Invoice> IInvoiceService.GetInvoicesByCustomerId(int customerId)
        {
            if (customerId == 0)
                return new List<Invoice>();

            var query = from o in _invoiceRepository.Table
                        where o.CustomerId == customerId
                        select o;

            return query.ToList();
        }

        public Invoice GetInvoicesByCustomerOrderId(int companyId, int saleOrderId)
        {
            if (companyId == 0)
                return new Invoice();

            var query = from o in _invoiceRepository.Table
                        where o.CompanyId == companyId && o.SaleOrderId == saleOrderId
                        select o;

                return query.FirstOrDefault();
          

        }
        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertInvoice(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            _invoiceRepository.Insert(invoice);

            //event notification
            _eventPublisher.EntityInserted(invoice);
        }

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void UpdateOrder(Invoice invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            _invoiceRepository.Update(invoice);

            //event notification
            _eventPublisher.EntityUpdated(invoice);
        }

        public Invoice GetInvoicesByTransId(int companyId, string transId)
        {
            if (companyId == 0)
                return new Invoice();

            var query = from o in _invoiceRepository.Table
                        where o.CompanyId == companyId && o.InvoiceNo == transId
                        select o;

            return query.FirstOrDefault();
        }

        IList<Invoice> IInvoiceService.GetInvoicesByCompanyId(int CompanyId)
        {
            if (CompanyId == 0)
                return new List<Invoice>();

            var query = from o in _invoiceRepository.Table
                        where o.CompanyId == CompanyId
                        select o;

            return query.ToList();
        }

        public virtual Invoice GetInvoiceById(int orderId)
        {
            if (orderId == 0)
                return null;

            return _invoiceRepository.Table.Where(r => r.Id == orderId).FirstOrDefault();
        }

        public virtual IList<Invoice> GetOrdersByIds(int[] orderIds)
        {
            if (orderIds == null || orderIds.Length == 0)
                return new List<Invoice>();

            var query = from o in _invoiceRepository.Table
                        where orderIds.Contains(o.Id)
                        select o;
            var orders = query.ToList();
            //sort by passed identifiers
            var sortedOrders = new List<Invoice>();
            foreach (var id in orderIds)
            {
                var order = orders.Find(x => x.Id == id);
                if (order != null)
                    sortedOrders.Add(order);
            }

            return sortedOrders;
        }


        public virtual IPagedList<InvoiceTransaction> SearchInvoice(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            var query = _InvoiceTransaction.Table;




            //if (companyId > 0)
            //    query = query.Where(o => o.CompanyId == companyId);

            //if (query.Count() == 0)
            //    query = queryInfo;

            //if (productId > 0)
            //   query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.ProductId == productId));

            //if (warehouseId > 0)
            //{
            //    var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
            //    query = query
            //        .Where(o => o.OrderItems
            //        .Any(orderItem =>
            //            //"Use multiple warehouses" enabled
            //            //we search in each warehouse
            //            (orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
            //            orderItem.Product.UseMultipleWarehouses &&
            //            orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
            //            ||
            //            //"Use multiple warehouses" disabled
            //            //we use standard "warehouse" property
            //            ((orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
            //            !orderItem.Product.UseMultipleWarehouses) &&
            //            orderItem.Product.WarehouseId == warehouseId)));
            //}

            //if (billingCountryId > 0)
            //    query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            //if (!string.IsNullOrEmpty(paymentMethodSystemName))
            //    query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            //if (affiliateId > 0)
            //    query = query.Where(o => o.AffiliateId == affiliateId);
            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedDate);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedDate);
            //if (osIds != null && osIds.Any())
            //{
            //    foreach (var item in osIds)
            //    {
            //        if (item.ToString() == "1")
            //        {
            //            query = query.Where(o => o.StatusPaymentNP==false);
            //        }

            //        if (item.ToString() == "2")
            //        {
            //            query = query.Where(o => o.StatusPaymentNP == true);
            //        }

            //    }
            //}
            //if (psIds != null && psIds.Any())
            //    query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            //if (ssIds != null && ssIds.Any())
            //    query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            //if (!string.IsNullOrEmpty(billingPhone))
            //    query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone));
            //if (!string.IsNullOrEmpty(billingEmail))
            //    query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            //if (!string.IsNullOrEmpty(billingLastName))
            //    query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            //if (!string.IsNullOrEmpty(orderNotes))
            //    query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));

            //query = query.Where(o => !o.Deleted);
            //query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return new PagedList<InvoiceTransaction>(query, pageIndex, pageSize, getOnlyTotalCount);
        }


        public virtual IList<InvoiceTransaction> GetTransactionByIds(int[] orderIds)
        {
            if (orderIds == null || orderIds.Length == 0)
                return new List<InvoiceTransaction>();

            var query = from o in _InvoiceTransaction.Table
                        where orderIds.Contains(o.Id)
                        select o;
            var orders = query.ToList();
            //sort by passed identifiers
            var sortedOrders = new List<InvoiceTransaction>();
            foreach (var id in orderIds)
            {
                var order = orders.Find(x => x.Id == id);
                if (order != null)
                    sortedOrders.Add(order);
            }

            return sortedOrders;
        }

        public virtual InvoiceTransaction GetTransactionById(int Id)
        {
            if (Id == 0)
                return null;

            var query = from o in _InvoiceTransaction.Table
                        where o.Id== Id
                        select o;
            var transactions = query.FirstOrDefault();
            

            return transactions;
        }
        #endregion


        #region Transactions Invoice
        public virtual void InsertInvoiceTransaction(InvoiceTransaction invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            _InvoiceTransaction.Insert(invoice);

            //event notification
            _eventPublisher.EntityInserted(invoice);
        }
        #endregion

    }


}
