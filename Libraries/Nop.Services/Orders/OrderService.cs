using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Invoices;
using Nop.Core.Domain.Orders;
using Nop.Core.Html;
using Nop.Services.Events;

namespace Nop.Services.Orders
{
    /// <summary>
    /// Order service
    /// </summary>
    public partial class OrderService : IOrderService
    {
        #region Fields

        private readonly IEventPublisher _eventPublisher;
        private readonly IRepository<Customer> _customerRepository;
        private readonly IRepository<Order> _orderRepository;
        private readonly IRepository<OrderItem> _orderItemRepository;
        private readonly IRepository<OrderNote> _orderNoteRepository;
        private readonly IRepository<Product> _productRepository;
        private readonly IRepository<RecurringPayment> _recurringPaymentRepository;
        private readonly IWorkContext _workContext;
        private readonly IRepository<Invoice> _invoiceRepository;

        #endregion

        #region Ctor

        public OrderService(IEventPublisher eventPublisher,
            IRepository<Customer> customerRepository,
            IRepository<Order> orderRepository,
            IRepository<OrderItem> orderItemRepository,
            IRepository<OrderNote> orderNoteRepository,
            IRepository<Product> productRepository,
            IRepository<RecurringPayment> recurringPaymentRepository,
            IWorkContext workContext, IRepository<Invoice> invoiceRepository)
        {
            _eventPublisher = eventPublisher;
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
            _orderItemRepository = orderItemRepository;
            _orderNoteRepository = orderNoteRepository;
            _productRepository = productRepository;
            _recurringPaymentRepository = recurringPaymentRepository;
            _workContext = workContext;
            _invoiceRepository = invoiceRepository;
        }

        #endregion

        #region Methods

        #region Orders

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderById(int orderId)
        {
            if (orderId == 0)
                return null;

            return _orderRepository.Table.Where(r => r.Id == orderId).FirstOrDefault();
        }

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="customOrderNumber">The custom order number</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByCustomOrderNumber(string customOrderNumber)
        {
            if (string.IsNullOrEmpty(customOrderNumber))
                return null;

            return _orderRepository.Table.FirstOrDefault(o => o.CustomOrderNumber == customOrderNumber);
        }

        /// <summary>
        /// Get orders by identifiers
        /// </summary>
        /// <param name="orderIds">Order identifiers</param>
        /// <returns>Order</returns>
        public virtual IList<Order> GetOrdersByIds(int[] orderIds)
        {
            if (orderIds == null || orderIds.Length == 0)
                return new List<Order>();

            var query = from o in _orderRepository.Table
                        where orderIds.Contains(o.Id) && !o.Deleted
                        select o;
            var orders = query.ToList();
            //sort by passed identifiers
            var sortedOrders = new List<Order>();
            foreach (var id in orderIds)
            {
                var order = orders.Find(x => x.Id == id);
                if (order != null)
                    sortedOrders.Add(order);
            }

            return sortedOrders;
        }

     

        /// <summary>
        /// Gets an order
        /// </summary>
        /// <param name="orderGuid">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByGuid(Guid orderGuid)
        {
            if (orderGuid == Guid.Empty)
                return null;

            var query = from o in _orderRepository.Table
                        where o.OrderGuid == orderGuid
                        select o;
            var order = query.FirstOrDefault();
            return order;
        }

        /// <summary>
        /// Deletes an order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void DeleteOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            order.Deleted = true;
            UpdateOrder(order);

            //event notification
            _eventPublisher.EntityDeleted(order);
        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; null to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; null to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="osIds">Order status identifiers; null to load all orders</param>
        /// <param name="psIds">Payment status identifiers; null to load all orders</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all orders</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// <returns>Orders</returns>
        public virtual IPagedList<Order> SearchOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null, List<int> CompanyIds = null, List<int> UserRoleId = null , string SourceName=null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false)
        {
            return OverriddenSearchOrders(storeId, vendorId, customerId, productId, affiliateId, warehouseId,
            billingCountryId, paymentMethodSystemName, createdFromUtc, createdToUtc,
            osIds, psIds, ssIds, CompanyIds, UserRoleId, SourceName, billingPhone, billingEmail, billingLastName, orderNotes, pageIndex, pageSize, getOnlyTotalCount, 0, false);
        }

        /// <summary>
        /// Search orders
        /// </summary>
        /// <param name="storeId">Store identifier; 0 to load all orders</param>
        /// <param name="vendorId">Vendor identifier; null to load all orders</param>
        /// <param name="customerId">Customer identifier; 0 to load all orders</param>
        /// <param name="productId">Product identifier which was purchased in an order; 0 to load all orders</param>
        /// <param name="affiliateId">Affiliate identifier; 0 to load all orders</param>
        /// <param name="billingCountryId">Billing country identifier; 0 to load all orders</param>
        /// <param name="warehouseId">Warehouse identifier, only orders with products from a specified warehouse will be loaded; 0 to load all orders</param>
        /// <param name="paymentMethodSystemName">Payment method system name; null to load all records</param>
        /// <param name="createdFromUtc">Created date from (UTC); null to load all records</param>
        /// <param name="createdToUtc">Created date to (UTC); null to load all records</param>
        /// <param name="osIds">Order status identifiers; null to load all orders</param>
        /// <param name="psIds">Payment status identifiers; null to load all orders</param>
        /// <param name="ssIds">Shipping status identifiers; null to load all orders</param>
        /// <param name="billingPhone">Billing phone. Leave empty to load all records.</param>
        /// <param name="billingEmail">Billing email. Leave empty to load all records.</param>
        /// <param name="billingLastName">Billing last name. Leave empty to load all records.</param>
        /// <param name="orderNotes">Search in order notes. Leave empty to load all records.</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">A value in indicating whether you want to load only total number of records. Set to "true" if you don't want to load data from database</param>
        /// /// <param name="getCompanyOrdersAssociatedToCustomer">Set to true to load all orders associated with the customer compay</param>
        /// <param name="companyId">Id from company</param>
        /// <returns>Orders</returns>
        public virtual IPagedList<Order> OverriddenSearchOrders(int storeId = 0,
            int vendorId = 0, int customerId = 0,
            int productId = 0, int affiliateId = 0, int warehouseId = 0,
            int billingCountryId = 0, string paymentMethodSystemName = null,
            DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
            List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null, List<int> CompanyIds = null, List<int> UserRoleId = null, string SourceName = null,
            string billingPhone = null, string billingEmail = null, string billingLastName = "",
            string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int companyId = 0,  bool getCompanyOrdersAssociatedToCustomer = false)
        {
            var query = _orderRepository.Table;
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (vendorId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId));

            if (customerId > 0)
            {
                if(_workContext.CurrentCustomer.IsRegistered() &&  _workContext.CurrentCustomer.Companies.Any())
                {
                    if (_workContext.WorkingCompany != null && getCompanyOrdersAssociatedToCustomer)
                        query = query.Where(o=>o.CompanyId == _workContext.WorkingCompany.Id);
                    else
                        query = query.Where(o => o.CustomerId == customerId);

                } else
                    query = query.Where(o => o.CustomerId == customerId);

            }
            
            var queryInfo = query;

            if (companyId > 0)
                query = query.Where(o => o.CompanyId == companyId);

            //if (query.Count() == 0)
            //    query = queryInfo;

            if (productId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.ProductId == productId));

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                        orderItem.Product.UseMultipleWarehouses &&
                        orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                        !orderItem.Product.UseMultipleWarehouses) &&
                        orderItem.Product.WarehouseId == warehouseId)));
            }

            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);
            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);
            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (!string.IsNullOrEmpty(billingPhone))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone));
            if (!string.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!string.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));


            if (CompanyIds != null && CompanyIds.Any())
                query = query.Where(o => CompanyIds.Contains((int)o.CompanyId));

            if (UserRoleId != null && UserRoleId.Any())
            {
                query = query.Where(o => o.Customer.CustomerCustomerRoleMappings.Any(cr => UserRoleId.Contains(cr.CustomerRoleId)));

    //            foreach (var item in UserRoleId)
				//{

    //            }

            }

            if(!string.IsNullOrEmpty(SourceName))
			{
                if (SourceName == "Web")
                {
                    query= query.Where(r => r.Source.Contains("REST"));

                }
                else if (SourceName == "NS")
                {
                    query = query.Where(r => string.IsNullOrEmpty(r.Source));

                }
                
            }
            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            query = query.Where(o => o.CustomOrderNumber != o.tranId);

            //database layer paging
            return new PagedList<Order>(query, pageIndex, pageSize, getOnlyTotalCount);
        }

        public virtual IPagedList<Order> OverriddenSearchOrdersParent(int storeId = 0,
        int vendorId = 0, int customerId = 0,
        int productId = 0, int affiliateId = 0, int warehouseId = 0,
        int billingCountryId = 0, string paymentMethodSystemName = null,
        DateTime? createdFromUtc = null, DateTime? createdToUtc = null,
        List<int> osIds = null, List<int> psIds = null, List<int> ssIds = null,
        string billingPhone = null, string billingEmail = null, string billingLastName = "",
        string orderNotes = null, int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, int companyId = 0, List<Company> parent = null, bool getCompanyOrdersAssociatedToCustomer = false)
        {
            var query = _orderRepository.Table;
            if (storeId > 0)
                query = query.Where(o => o.StoreId == storeId);
            if (vendorId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId));

            query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.Product.VendorId == vendorId));



            if (customerId > 0)
            {
               query = query.Where(o => o.CustomerId == customerId);
            }

            if (companyId > 0)
                query = query.Where(o => o.CompanyId == companyId);

            if (productId > 0)
                query = query.Where(o => o.OrderItems.Any(orderItem => orderItem.ProductId == productId));

            if (warehouseId > 0)
            {
                var manageStockInventoryMethodId = (int)ManageInventoryMethod.ManageStock;
                query = query
                    .Where(o => o.OrderItems
                    .Any(orderItem =>
                        //"Use multiple warehouses" enabled
                        //we search in each warehouse
                        (orderItem.Product.ManageInventoryMethodId == manageStockInventoryMethodId &&
                        orderItem.Product.UseMultipleWarehouses &&
                        orderItem.Product.ProductWarehouseInventory.Any(pwi => pwi.WarehouseId == warehouseId))
                        ||
                        //"Use multiple warehouses" disabled
                        //we use standard "warehouse" property
                        ((orderItem.Product.ManageInventoryMethodId != manageStockInventoryMethodId ||
                        !orderItem.Product.UseMultipleWarehouses) &&
                        orderItem.Product.WarehouseId == warehouseId)));
            }

            if (billingCountryId > 0)
                query = query.Where(o => o.BillingAddress != null && o.BillingAddress.CountryId == billingCountryId);
            if (!string.IsNullOrEmpty(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);
            if (affiliateId > 0)
                query = query.Where(o => o.AffiliateId == affiliateId);
            if (createdFromUtc.HasValue)
                query = query.Where(o => createdFromUtc.Value <= o.CreatedOnUtc);
            if (createdToUtc.HasValue)
                query = query.Where(o => createdToUtc.Value >= o.CreatedOnUtc);
            if (osIds != null && osIds.Any())
                query = query.Where(o => osIds.Contains(o.OrderStatusId));
            if (psIds != null && psIds.Any())
                query = query.Where(o => psIds.Contains(o.PaymentStatusId));
            if (ssIds != null && ssIds.Any())
                query = query.Where(o => ssIds.Contains(o.ShippingStatusId));
            if (!string.IsNullOrEmpty(billingPhone))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.PhoneNumber) && o.BillingAddress.PhoneNumber.Contains(billingPhone));
            if (!string.IsNullOrEmpty(billingEmail))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.Email) && o.BillingAddress.Email.Contains(billingEmail));
            if (!string.IsNullOrEmpty(billingLastName))
                query = query.Where(o => o.BillingAddress != null && !string.IsNullOrEmpty(o.BillingAddress.LastName) && o.BillingAddress.LastName.Contains(billingLastName));
            if (!string.IsNullOrEmpty(orderNotes))
                query = query.Where(o => o.OrderNotes.Any(on => on.Note.Contains(orderNotes)));

            query = query.Where(o => !o.Deleted);
            query = query.OrderByDescending(o => o.CreatedOnUtc);

            //database layer paging
            return new PagedList<Order>(query, pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository.Insert(order);

            //event notification
            _eventPublisher.EntityInserted(order);
        }

        /// <summary>
        /// Updates the order
        /// </summary>
        /// <param name="order">The order</param>
        public virtual void UpdateOrder(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            _orderRepository.Update(order);

            //event notification
            _eventPublisher.EntityUpdated(order);
        }

        /// <summary>
        /// Get an order by authorization transaction ID and payment method system name
        /// </summary>
        /// <param name="authorizationTransactionId">Authorization transaction ID</param>
        /// <param name="paymentMethodSystemName">Payment method system name</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByAuthorizationTransactionIdAndPaymentMethod(string authorizationTransactionId,
            string paymentMethodSystemName)
        {
            var query = _orderRepository.Table;
            if (!string.IsNullOrWhiteSpace(authorizationTransactionId))
                query = query.Where(o => o.AuthorizationTransactionId == authorizationTransactionId);

            if (!string.IsNullOrWhiteSpace(paymentMethodSystemName))
                query = query.Where(o => o.PaymentMethodSystemName == paymentMethodSystemName);

            query = query.OrderByDescending(o => o.CreatedOnUtc);
            var order = query.FirstOrDefault();
            return order;
        }

        /// <summary>
        /// Parse tax rates
        /// </summary>
        /// <param name="order">Order</param>
        /// <param name="taxRatesStr"></param>
        /// <returns>Rates</returns>
        public virtual SortedDictionary<decimal, decimal> ParseTaxRates(Order order, string taxRatesStr)
        {
            var taxRatesDictionary = new SortedDictionary<decimal, decimal>();

            if (string.IsNullOrEmpty(taxRatesStr))
                return taxRatesDictionary;

            var lines = taxRatesStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line.Trim()))
                    continue;

                var taxes = line.Split(':');
                if (taxes.Length != 2)
                    continue;

                try
                {
                    var taxRate = decimal.Parse(taxes[0].Trim(), CultureInfo.InvariantCulture);
                    var taxValue = decimal.Parse(taxes[1].Trim(), CultureInfo.InvariantCulture);
                    taxRatesDictionary.Add(taxRate, taxValue);
                }
                catch
                {
                    // ignored
                }
            }

            //add at least one tax rate (0%)
            if (!taxRatesDictionary.Any())
                taxRatesDictionary.Add(decimal.Zero, decimal.Zero);

            return taxRatesDictionary;
        }

        /// <summary>
        /// Gets a value indicating whether an order has items to be added to a shipment
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether an order has items to be added to a shipment</returns>
        public virtual bool HasItemsToAddToShipment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            foreach (var orderItem in order.OrderItems)
            {
                //we can ship only shippable products
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                var totalNumberOfItemsCanBeAddedToShipment = GetTotalNumberOfItemsCanBeAddedToShipment(orderItem);
                if (totalNumberOfItemsCanBeAddedToShipment <= 0)
                    continue;

                //yes, we have at least one item to create a new shipment
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether an order has items to ship
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether an order has items to ship</returns>
        public virtual bool HasItemsToShip(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            foreach (var orderItem in order.OrderItems)
            {
                //we can ship only shippable products
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                var totalNumberOfNotYetShippedItems = GetTotalNumberOfNotYetShippedItems(orderItem);
                if (totalNumberOfNotYetShippedItems <= 0)
                    continue;

                //yes, we have at least one item to ship
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets a value indicating whether an order has items to deliver
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>A value indicating whether an order has items to deliver</returns>
        public virtual bool HasItemsToDeliver(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            foreach (var orderItem in order.OrderItems)
            {
                //we can ship only shippable products
                if (!orderItem.Product.IsShipEnabled)
                    continue;

                var totalNumberOfShippedItems = GetTotalNumberOfShippedItems(orderItem);
                var totalNumberOfDeliveredItems = GetTotalNumberOfDeliveredItems(orderItem);
                if (totalNumberOfShippedItems <= totalNumberOfDeliveredItems)
                    continue;

                //yes, we have at least one item to deliver
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets an order by netsuiteId
        /// </summary>
        /// <param name="orderId">The order identifier</param>
        /// <returns>Order</returns>
        public virtual Order GetOrderByNetsuiteId(string tranId)
        {
            if (string.IsNullOrEmpty(tranId))
                return null;

            var query = from o in _orderRepository.Table
                        where o.tranId == tranId
                        select o;
            var order = query.FirstOrDefault();
            return order;
        }

        public virtual List<Order> GetOrderByCustomer(int id)
        {
            if (id == 0)
                return null;

            var query = from o in _orderRepository.Table
                        where o.CustomerId == id && o.tranId==null 
                        select o;
            var order = query.ToList();
            return order;
        }

         public virtual List<Order> GetOrderFromNetsuitePendingFulfillment(int id)
        {
            if (id == 0)
                return null;

            var query = from o in _orderRepository.Table
                        where o.CompanyId == id && (o.OrderStatus==OrderStatus.Pending || o.OrderStatus == OrderStatus.ReadyPickup || o.OrderStatus == OrderStatus.ReadyDelivery
                        || o.OrderStatus == OrderStatus.ReadyUps || o.OrderStatus == OrderStatus.Pending || o.OrderStatus == OrderStatus.Processing
                        || o.OrderStatus == OrderStatus.BillingPartiallyFulfilled || o.OrderStatus == OrderStatus.PendingPartiallyFulfilled || o.OrderStatus == OrderStatus.PartiallyFulfilled)

                        select o;
            var order = query.ToList();
            return order;
        }

        public virtual List<Order> GetOrderByCompanyByPendingBilling(int id)
        {
            if (id == 0)
                return null;

            var query = from o in _orderRepository.Table
                        where o.CompanyId == id && (o.OrderStatus==OrderStatus.ReadyPickup || o.OrderStatus == OrderStatus.ReadyDelivery
                        || o.OrderStatus == OrderStatus.ReadyUps || o.OrderStatus == OrderStatus.Pending  || o.OrderStatus == OrderStatus.Processing || o.OrderStatus == OrderStatus.Complete
                        || o.OrderStatus == OrderStatus.BillingPartiallyFulfilled || o.OrderStatus == OrderStatus.PendingPartiallyFulfilled || o.OrderStatus == OrderStatus.PartiallyFulfilled
                        || o.OrderStatus == OrderStatus.Closed)
                        select o;
            var order = query.ToList();
            return order;
        }

        public virtual List<Order> GetOrderByCompanyByBilled(int id)
        {
            if (id == 0)
                return null;

            var query = from o in _orderRepository.Table
                        where o.CompanyId == id && (o.OrderStatus == OrderStatus.CompletedBilled || o.OrderStatus == OrderStatus.Complete)
                        select o;
            var order = query.ToList();
            return order;
        }

        public virtual List<Order> GetOrderByEmail(string email, string orderId)
        {
            int numero=0;

            if (string.IsNullOrEmpty(email))
                return null;

            if (string.IsNullOrEmpty(orderId))
                return null;

            bool EsEntero = Int32.TryParse(orderId, out numero);

            if (EsEntero)
                numero = Convert.ToInt32(orderId);

            var query = from o in _orderRepository.Table
                        where o.Customer.BillingAddress.Email == email && (o.CustomOrderNumber.Contains(orderId) || (o.tranId != null && o.tranId.Contains(orderId)) || o.Id== numero)
                        select o;
            var order = query.ToList();

            if (order.Count == 0)
            {
                query = from o in _orderRepository.Table
                        where o.Customer.ShippingAddress.Email == email && (o.CustomOrderNumber.Contains(orderId) || (o.tranId != null && o.tranId.Contains(orderId)) || o.Id == numero)
                        select o;
                 order = query.ToList();
            }

            if (order.Count == 0)
            {
                query = from o in _orderRepository.Table
                        where o.Customer.Email == email && (o.CustomOrderNumber.Contains(orderId) || (o.tranId != null && o.tranId.Contains(orderId)) || o.Id == numero)
                        select o;
                order = query.ToList();
            }
            return order;
        }

        #endregion

        #region Orders items

        /// <summary>
        /// Gets an order item
        /// </summary>
        /// <param name="orderItemId">Order item identifier</param>
        /// <returns>Order item</returns>
        public virtual OrderItem GetOrderItemById(int orderItemId)
        {
            if (orderItemId == 0)
                return null;

            return _orderItemRepository.GetById(orderItemId);
        }

        public virtual ICollection<OrderItem> GetOrderItemByOrderId(int orderId)
        {
            if (orderId == 0)
                return new List<OrderItem>();

            return _orderItemRepository.Table.Where(r=>r.OrderId==orderId).ToList();
        }

        

        /// <summary>
        /// Gets an item
        /// </summary>
        /// <param name="orderItemGuid">Order identifier</param>
        /// <returns>Order item</returns>
        public virtual OrderItem GetOrderItemByGuid(Guid orderItemGuid)
        {
            if (orderItemGuid == Guid.Empty)
                return null;

            var query = from orderItem in _orderItemRepository.Table
                        where orderItem.OrderItemGuid == orderItemGuid
                        select orderItem;
            var item = query.FirstOrDefault();
            return item;
        }

        /// <summary>
        /// Gets all downloadable order items
        /// </summary>
        /// <param name="customerId">Customer identifier; null to load all records</param>
        /// <returns>Order items</returns>
        public virtual IList<OrderItem> GetDownloadableOrderItems(int customerId)
        {
            if (customerId == 0)
                throw new ArgumentOutOfRangeException(nameof(customerId));

            var query = from orderItem in _orderItemRepository.Table
                        join o in _orderRepository.Table on orderItem.OrderId equals o.Id
                        join p in _productRepository.Table on orderItem.ProductId equals p.Id
                        where customerId == o.CustomerId &&
                        p.IsDownload &&
                        !o.Deleted
                        orderby o.CreatedOnUtc descending, orderItem.Id
                        select orderItem;

            var orderItems = query.ToList();
            return orderItems;
        }

        /// <summary>
        /// Delete an order item
        /// </summary>
        /// <param name="orderItem">The order item</param>
        public virtual void DeleteOrderItem(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            _orderItemRepository.Delete(orderItem);

            //event notification
            _eventPublisher.EntityDeleted(orderItem);
        }

        /// <summary>
        /// Gets a total number of items in all shipments
        /// </summary>
        /// <param name="orderItem">Order item</param>
        /// <returns>Total number of items in all shipments</returns>
        public virtual int GetTotalNumberOfItemsInAllShipment(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var totalInShipments = 0;
            var shipments = orderItem.Order.Shipments.ToList();
            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                var si = shipment.ShipmentItems
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    totalInShipments += si.Quantity;
                }
            }

            return totalInShipments;
        }

        /// <summary>
        /// Gets a total number of already items which can be added to new shipments
        /// </summary>
        /// <param name="orderItem">Order item</param>
        /// <returns>Total number of already delivered items which can be added to new shipments</returns>
        public virtual int GetTotalNumberOfItemsCanBeAddedToShipment(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var totalInShipments = GetTotalNumberOfItemsInAllShipment(orderItem);

            var qtyOrdered = orderItem.Quantity;
            var qtyCanBeAddedToShipmentTotal = qtyOrdered - totalInShipments;
            if (qtyCanBeAddedToShipmentTotal < 0)
                qtyCanBeAddedToShipmentTotal = 0;

            return qtyCanBeAddedToShipmentTotal;
        }

        /// <summary>
        /// Gets a total number of not yet shipped items (but added to shipments)
        /// </summary>
        /// <param name="orderItem">Order item</param>
        /// <returns>Total number of not yet shipped items (but added to shipments)</returns>
        public virtual int GetTotalNumberOfNotYetShippedItems(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var result = 0;
            var shipments = orderItem.Order.Shipments.ToList();
            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                if (shipment.ShippedDateUtc.HasValue)
                    //already shipped
                    continue;

                var si = shipment.ShipmentItems
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    result += si.Quantity;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a total number of already shipped items
        /// </summary>
        /// <param name="orderItem">Order item</param>
        /// <returns>Total number of already shipped items</returns>
        public virtual int GetTotalNumberOfShippedItems(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var result = 0;
            var shipments = orderItem.Order.Shipments.ToList();
            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                if (!shipment.ShippedDateUtc.HasValue)
                    //not shipped yet
                    continue;

                var si = shipment.ShipmentItems
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    result += si.Quantity;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets a total number of already delivered items
        /// </summary>
        /// <param name="orderItem">Order  item</param>
        /// <returns>Total number of already delivered items</returns>
        public virtual int GetTotalNumberOfDeliveredItems(OrderItem orderItem)
        {
            if (orderItem == null)
                throw new ArgumentNullException(nameof(orderItem));

            var result = 0;
            var shipments = orderItem.Order.Shipments.ToList();
            for (var i = 0; i < shipments.Count; i++)
            {
                var shipment = shipments[i];
                if (!shipment.DeliveryDateUtc.HasValue)
                    //not delivered yet
                    continue;

                var si = shipment.ShipmentItems
                    .FirstOrDefault(x => x.OrderItemId == orderItem.Id);
                if (si != null)
                {
                    result += si.Quantity;
                }
            }

            return result;
        }

        #endregion

        #region Orders notes

        /// <summary>
        /// Gets an order note
        /// </summary>
        /// <param name="orderNoteId">The order note identifier</param>
        /// <returns>Order note</returns>
        public virtual OrderNote GetOrderNoteById(int orderNoteId)
        {
            if (orderNoteId == 0)
                return null;

            return _orderNoteRepository.GetById(orderNoteId);
        }

        /// <summary>
        /// Deletes an order note
        /// </summary>
        /// <param name="orderNote">The order note</param>
        public virtual void DeleteOrderNote(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException(nameof(orderNote));

            _orderNoteRepository.Delete(orderNote);

            //event notification
            _eventPublisher.EntityDeleted(orderNote);
        }

        /// <summary>
        /// Formats the order note text
        /// </summary>
        /// <param name="orderNote">Order note</param>
        /// <returns>Formatted text</returns>
        public virtual string FormatOrderNoteText(OrderNote orderNote)
        {
            if (orderNote == null)
                throw new ArgumentNullException(nameof(orderNote));

            var text = orderNote.Note;

            if (string.IsNullOrEmpty(text))
                return string.Empty;

            text = HtmlHelper.FormatText(text, false, true, false, false, false, false);

            return text;
        }

        #endregion

        #region Recurring payments

        /// <summary>
        /// Deletes a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void DeleteRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException(nameof(recurringPayment));

            recurringPayment.Deleted = true;
            UpdateRecurringPayment(recurringPayment);

            //event notification
            _eventPublisher.EntityDeleted(recurringPayment);
        }

        /// <summary>
        /// Gets a recurring payment
        /// </summary>
        /// <param name="recurringPaymentId">The recurring payment identifier</param>
        /// <returns>Recurring payment</returns>
        public virtual RecurringPayment GetRecurringPaymentById(int recurringPaymentId)
        {
            if (recurringPaymentId == 0)
                return null;

            return _recurringPaymentRepository.GetById(recurringPaymentId);
        }

        /// <summary>
        /// Inserts a recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void InsertRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException(nameof(recurringPayment));

            _recurringPaymentRepository.Insert(recurringPayment);

            //event notification
            _eventPublisher.EntityInserted(recurringPayment);
        }

        /// <summary>
        /// Updates the recurring payment
        /// </summary>
        /// <param name="recurringPayment">Recurring payment</param>
        public virtual void UpdateRecurringPayment(RecurringPayment recurringPayment)
        {
            if (recurringPayment == null)
                throw new ArgumentNullException(nameof(recurringPayment));

            _recurringPaymentRepository.Update(recurringPayment);

            //event notification
            _eventPublisher.EntityUpdated(recurringPayment);
        }

        /// <summary>
        /// Search recurring payments
        /// </summary>
        /// <param name="storeId">The store identifier; 0 to load all records</param>
        /// <param name="customerId">The customer identifier; 0 to load all records</param>
        /// <param name="initialOrderId">The initial order identifier; 0 to load all records</param>
        /// <param name="initialOrderStatus">Initial order status identifier; null to load all records</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="showHidden">A value indicating whether to show hidden records</param>
        /// <returns>Recurring payments</returns>
        public virtual IPagedList<RecurringPayment> SearchRecurringPayments(int storeId = 0,
            int customerId = 0, int initialOrderId = 0, OrderStatus? initialOrderStatus = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool showHidden = false)
        {
            int? initialOrderStatusId = null;
            if (initialOrderStatus.HasValue)
                initialOrderStatusId = (int)initialOrderStatus.Value;

            var query1 = from rp in _recurringPaymentRepository.Table
                         join c in _customerRepository.Table on rp.InitialOrder.CustomerId equals c.Id
                         where
                         !rp.Deleted &&
                         (showHidden || !rp.InitialOrder.Deleted) &&
                         (showHidden || !c.Deleted) &&
                         (showHidden || rp.IsActive) &&
                         (customerId == 0 || rp.InitialOrder.CustomerId == customerId) &&
                         (storeId == 0 || rp.InitialOrder.StoreId == storeId) &&
                         (initialOrderId == 0 || rp.InitialOrder.Id == initialOrderId) &&
                         (!initialOrderStatusId.HasValue || initialOrderStatusId.Value == 0 ||
                          rp.InitialOrder.OrderStatusId == initialOrderStatusId.Value)
                         select rp.Id;

            var query2 = from rp in _recurringPaymentRepository.Table
                         where query1.Contains(rp.Id)
                         orderby rp.StartDateUtc, rp.Id
                         select rp;

            var recurringPayments = new PagedList<RecurringPayment>(query2, pageIndex, pageSize);
            return recurringPayments;
        }

        #endregion


        #region Invoices
        public virtual IList<Invoice> GetOpenInvoice(int companyId)
        {
            if (companyId == 0)
                return new List<Invoice>();

            var query = from o in _invoiceRepository.Table
                        where o.CompanyId == companyId && o.Status == "A"
                        select o;
            var orders = query.ToList();

            return orders;
        }


        public virtual Invoice GetInvoiceById(int orderId)
        {
            if (orderId == 0)
                return null;

            return _invoiceRepository.Table.Where(r => r.Id == orderId).FirstOrDefault();
        }
        public virtual List<Invoice> GetInvoiceByOrderId(int orderId)
        {
            if (orderId == 0)
                return null;

            return _invoiceRepository.Table.Where(r => r.SaleOrderId == orderId).ToList();
        }

        #endregion
        #endregion
    }
}