using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Invoices;
using Nop.Core.Domain.NN;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order;
using Nop.Plugin.Misc.NetSuiteConnector.Models.Order;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Order;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Invoices;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.NN;
using Nop.Services.Shipping;
using System;
using System.Collections.Generic;
using System.Linq;
using Item = Nop.Plugin.Misc.NetSuiteConnector.Models.Order.Item;
using Location = Nop.Plugin.Misc.NetSuiteConnector.Models.Order.Location;
using OrderStatus = Nop.Core.Domain.Orders.OrderStatus;
using Order = Nop.Core.Domain.Orders.Order;
using Nop.Web.Models.Order;
using Nop.Core.Domain.Invoice;
using Nop.Services.Payments;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Address;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public partial class OrderService : IOrderService
    {
        #region Fields

        private readonly Nop.Services.Customers.ICustomerService _customerService;
        private readonly IConnectionServices _connectionService;
        private readonly Nop.Services.Orders.IOrderService _orderService;
        private readonly IInvoiceService _invoice;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly Nop.Services.Catalog.IProductService _productService;
        private readonly IProductService _productServiceNetsuite;
        private readonly ILogger _logger;
        private readonly ISettingService _settingService;
        private readonly IPendingOrdersToSyncService _pendingOrdersToSyncService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly Nop.Services.Customers.ICompanyService _companyService;

        private readonly ICreditManageService _creditManageService;

        private readonly INotificationService _notificationService;
        private readonly IRepository<DeliveryRoutes> _repository;

        private readonly IShippingService _shippingService;
        private IPendingDataToSyncService _pendingDataToSyncService;

        private bool TestMode = false;
        private string ModeAbb = "NC_";
        private DateTime LastExecutionDateGeneral;
        #endregion

        #region Ctor

        public OrderService(IConnectionServices connectionService, IInvoiceService invoice,
            Nop.Services.Orders.IOrderService orderService, 
            IStoreContext storeContext, Nop.Services.Catalog.IProductService productService, IInvoicePaymentService InvoicePayment,
            IWorkContext workContext, IProductService productServiceNetsuite, ILogger logger, ISettingService settingService, Nop.Services.Customers.ICustomerService customerService,
            IPendingOrdersToSyncService pendingOrdersToSyncService, IGenericAttributeService genericAttributeService,
            INotificationService notificationService, Nop.Services.Customers.ICompanyService companyService, IRepository<DeliveryRoutes> repository, IShippingService shippingService,
            IPendingDataToSyncService pendingDataToSyncService, ICreditManageService creditManageService)
        {
            _connectionService = connectionService;
            _orderService = orderService;
            _storeContext = storeContext;
            _workContext = workContext;
            _productService = productService;
            _productServiceNetsuite = productServiceNetsuite;
            _invoice = invoice;
            _logger = logger;
            _settingService = settingService;
            _customerService = customerService;
            _pendingOrdersToSyncService = pendingOrdersToSyncService;
            _genericAttributeService = genericAttributeService;
            _companyService = companyService;
           
            _notificationService = notificationService;
            _repository = repository;
            _shippingService = shippingService;
            _pendingDataToSyncService = pendingDataToSyncService;
            _creditManageService = creditManageService;
        }

        #endregion

        #region Public Methods

        public void ImportOrders(string LastExecutionDate, string idorder = null, string type = null)
        {
            try
            {
                LastExecutionDateGeneral = DateTime.Now;

                TestMode = Convert.ToBoolean(_settingService.GetSetting("NetSuiteConnector.TestMode").Value);

                if (TestMode)
                    ModeAbb = "NCTest_";

               // SyncPendingOrders(LastExecutionDate);

                if (type == "All")
                {
                    ImportOrders(null);
                }
                else if (type == "LastUpdate")
                {
                    ImportOrders(LastExecutionDate);
                }
                else if (type == "SpecificCustomerId")
                {
                    ImportSpecifictCompany(idorder, LastExecutionDate);
                }
                else if (string.IsNullOrEmpty(type))
                {
                    ImportOrders(LastExecutionDate);
                }
            }
            catch (Exception)
            {

            }
        }

        public void ImportOrders(string LastExecutionDate, string idorder = null, string type = null, List<PendingDataToSync> listProducts = null)
        {
            try
            {
                LastExecutionDateGeneral = DateTime.Now;

                TestMode = Convert.ToBoolean(_settingService.GetSetting("NetSuiteConnector.TestMode").Value);

                if (TestMode)
                    ModeAbb = "NCTest_";

               // SyncPendingOrders(LastExecutionDate);

                if (type == "All")
                {
                    ImportOrders(null);
                }
                else if (type == "LastUpdate")
                {
                    ImportOrders(LastExecutionDate);
                }
                else if (type == "SpecificCustomerId")
                {
                    ImportSpecifictCompany(idorder, LastExecutionDate);
                }
                else if (type == "GetDataFromNetsuite")
                {

                    foreach (var item in listProducts)
                    {
                        try
                        {
                            _logger.Warning("Start update order: " + item.IdItem);

                            UpdateOrderFromApi(item.IdItem, item.ShippingStatus);

                            item.SuccessDate = DateTime.Now;
                            item.UpdateDate = DateTime.Now;
                            item.Synchronized = true;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                            _logger.Warning("Finished update order: " + item.IdItem);
                        }
                        catch (Exception ex)
                        {
                            item.UpdateDate = DateTime.Now;
                            item.Synchronized = false;
                            item.FailedDate = DateTime.Now;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                            _logger.Warning("Error update order: " + item.IdItem, ex);
                        }

                    }
                }
                else if (type == "GetInvoiceByCustomer")
                {

                    foreach (var item in listProducts)
                    {
                        try
                        {
                            _logger.Warning("Start update Invoice by customer: " + item.IdItem);

                            GetOpenInvoiceByCompanyId(item.IdItem, 0);

                            item.SuccessDate = DateTime.Now;
                            item.UpdateDate = DateTime.Now;
                            item.Synchronized = true;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                            _logger.Warning("Finished update Invoice by customer: " + item.IdItem);
                        }
                        catch (Exception ex)
                        {
                            item.UpdateDate = DateTime.Now;
                            item.Synchronized = false;
                            item.FailedDate = DateTime.Now;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                            _logger.Warning("Error update Invoice by customer: " + item.IdItem, ex);
                        }

                    }
                }
                //else if (type == "SendInvoicesDataToNetsuite")
                //{
                //    foreach (var item in listProducts)
                //    {
                //        try
                //        {
                //            _logger.Warning("Start send  payment: " + item.IdItem);

                //            SendInvoicesPaymentToNetsuite(item.IdItem);

                //            item.SuccessDate = DateTime.Now;
                //            item.UpdateDate = DateTime.Now;
                //            item.Synchronized = true;
                //            _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                //            _logger.Warning("Finished update order: " + item.IdItem);
                //        }
                //        catch (Exception ex)
                //        {
                //            item.UpdateDate = DateTime.Now;
                //            item.Synchronized = false;
                //            item.FailedDate = DateTime.Now;
                //            _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                //            _logger.Warning("Error send payment: " + item.IdItem, ex);
                //        }
                //    }
                //}
                else if (string.IsNullOrEmpty(type))
                {
                    ImportOrders(LastExecutionDate);
                }
            }
            catch (Exception)
            {

            }
        }

        public void ImportSpecifictCompany(string companyId, string LastExecutionDate)
        {
            if (companyId != null)
            {
                List<CompanyCustomers> ChildCompaniesWhitoutLogin = new List<CompanyCustomers>();
                List<CompanyCustomers> CompanyCustomers = new List<CompanyCustomers>();
                List<Company> CompanyLit = new List<Company>();
                List<Customer> CustomerList = new List<Customer>();

                var CompanyInfo = _companyService.GetCompanyByNetSuiteId(Convert.ToInt32(companyId)).FirstOrDefault();
                if (CompanyInfo != null)
                {
                    var Customers = _companyService.GetAllCompanyCustomerMappings(null, CompanyInfo.Id);

                    foreach (var item in Customers)
                    {
                        CompanyCustomers comp = new CompanyCustomers();
                        comp.Company = item;
                        comp.Customer.Add(item.Customer);
                        CompanyCustomers.Add(comp);
                        CustomerList.Add(item.Customer);
                    }

                    var childs = _companyService.GetCompanyChildByParentId(Convert.ToInt32(CompanyInfo.NetsuiteId));
                    if (childs.Count > 0)
                    {
                        foreach (var Childcompany in childs)
                        {
                            CompanyCustomers compChild = new CompanyCustomers();
                            compChild.Company = new CompanyCustomerMapping();

                            var ChildCompany = CompanyCustomers.Where(r => r.Company.CompanyId == Childcompany.Id).FirstOrDefault();

                            if (ChildCompany == null)
                            {
                                compChild.Company.Company = Childcompany;
                                compChild.Company.CompanyId = Convert.ToInt32(Childcompany?.Id);
                                compChild.Customer = Childcompany.Customers.ToList();
                                ChildCompaniesWhitoutLogin.Add(compChild);

                            }
                        }
                    }

                    UpdateOrderFromNetsuiteToNopcommerce(LastExecutionDate, CustomerList, CompanyInfo);

                    foreach (var company in ChildCompaniesWhitoutLogin)
                    {
                        GetOpenInvoiceByCompanyId(Convert.ToInt32(company.Company.Company.NetsuiteId), Convert.ToInt32(company.Company.Company.Id));
                    }
                }
                else
                {
                    if (companyId == _settingService.GetSetting("WebStoreCustomer.id").Value)
                    {
                        GetWebStoreCustomerOrders(LastExecutionDate);
                    }
                }

            }
            
        }
        public void ImportOrders(string LastExecutionDate)
        {
            
            var customerIds = _customerService.GetCustomerPasswords().Select(p => p.CustomerId);
            var customers = _customerService.GetCustomersByIds(customerIds.ToArray()).OrderByDescending(r=>r.LastLoginDateUtc);

            List<CompanyCustomers> CompanyCustomers = new List<CompanyCustomers>();
            
            List<CompanyCustomers> ChildCompaniesWhitoutLogin = new List<CompanyCustomers>();
            //Get order status A by customer loggin
            foreach (var custo in customers)
            {
                foreach (var item in custo.CompanyCustomerMappings)
                {
                    CompanyCustomers comp = new CompanyCustomers();

                    var ExistCompany = CompanyCustomers.Where(r => r.Company.CompanyId == item.CompanyId).FirstOrDefault();

                    if (ExistCompany == null)
                    {
                        comp.Company = item;
                        comp.Customer.Add(custo);
                        CompanyCustomers.Add(comp);

                      
                    }
                    else
                    {
                        var ExistContact = ExistCompany.Customer.Where(r => r.Id == custo.Id).FirstOrDefault();
                        if (ExistContact == null)
                        {
                            ExistCompany.Customer.Add(custo);
                        }
                    }
                    if (item.CompanyId == 4178)
                    {
                    }

                    //if (item.Company.NetsuiteId== "14398")
                    //{
                    GetOrderFromChildsCompany(CompanyCustomers, ChildCompaniesWhitoutLogin, custo, item);
                }
            }
            
            foreach (var company in CompanyCustomers)
            {
                UpdateOrderFromNetsuiteToNopcommerce(LastExecutionDate, company.Customer, company.Company?.Company);
            }


            foreach (var company in ChildCompaniesWhitoutLogin)
            {
                GetOpenInvoiceByCompanyId(Convert.ToInt32(company.Company.Company.NetsuiteId), Convert.ToInt32(company.Company.Company.Id));

            }
           
            // Get Web store Customer Orders
            GetWebStoreCustomerOrders(LastExecutionDate);

        }

        private void GetOrderFromChildsCompany(List<CompanyCustomers> CompanyCustomers, List<CompanyCustomers> ChildCompaniesWhitoutLogin, Customer custo, CompanyCustomerMapping item)
        {
            var childs = _companyService.GetCompanyChildByParentId(Convert.ToInt32(item.Company.NetsuiteId));
            if (childs.Count > 0)
            {
                foreach (var Childcompany in childs)
                {
                    CompanyCustomers compChild = new CompanyCustomers();
                    compChild.Company = new CompanyCustomerMapping();

                    var ChildCompany = CompanyCustomers.Where(r => r.Company.CompanyId == Childcompany.Id).FirstOrDefault();

                    if (ChildCompany == null)
                    {
                        compChild.Company.Company = Childcompany;
                        compChild.Company.CompanyId = Convert.ToInt32(Childcompany?.Id);
                        compChild.Customer.Add(custo);
                        ChildCompaniesWhitoutLogin.Add(compChild);
                    }
                }
            }
        }

        private void UpdateOrderFromNetsuiteToNopcommerce(string LastExecutionDate, List<Customer> customers, Company companyDetail)
        {
                GetTransactionInfoOpenAndPending(companyDetail, customers, LastExecutionDate);
                
               
            if (companyDetail.NetsuiteId != null)
            {
                GetOpenInvoiceByCompanyId(Convert.ToInt32(companyDetail.NetsuiteId), Convert.ToInt32(companyDetail.Id));

                //Get Credit memos, customer deposit, customer payments
                _creditManageService.GetCustomerBalance(Convert.ToInt32(companyDetail.NetsuiteId),LastExecutionDate);
            }
        }

        public void GetTransactionInfoOpenAndPending(Company comp, List<Customer> customer, string LastExecutionDate = null)
        {
            try
            {
                //1. Get open sales order
                var transactionList = new TransactionListDto();
                if (comp?.NetsuiteId != null)
                {
                    // Get orders Pending for approval
                    transactionList = GetPendingSalesbyNetsuite(Convert.ToInt32(comp.NetsuiteId), LastExecutionDate);
                    if (transactionList != null)
                    {
                        if (transactionList.Items.Count > 0)
                        {
                            foreach (var item in transactionList.Items)
                            {
                                if (item.Document == "SO111479")
                                {

                                }

                                foreach (var c in customer)
                                    {
                                        CreateOrderNopCommerceByNetsuite(c, item);
                                    }
                                    
                                
                                
                            }
                        }

                    }

                    // Get orders Pending Fulfillment 
                    var PendingFulfillmentOrderList = GetOpenSalesbyNetsuite(Convert.ToInt32(comp.NetsuiteId), LastExecutionDate);
                    if (PendingFulfillmentOrderList != null)
                    {
                        foreach (var item in PendingFulfillmentOrderList.Items)
                        {
                            if (item.Document == "SO114408")
                            {

                            }
                                foreach (var c in customer)
                                {
                                    CreateOrderNopCommerceByNetsuite(c, item);
                                }
                            
                        }
                    }

                    GetOrderFromNetsuitePendingAndBilled(comp.NetsuiteId, comp.Id, LastExecutionDate);
                }
                else
                {
                foreach (var c in customer)
                {

                    var orderPendingList = _orderService.GetOrderByCustomer(c.Id);

                    foreach (var x in orderPendingList)
                    {
                        var webid = _settingService.GetSetting("WebStoreCustomer.id").Value;
                        if (webid != null)
                            transactionList = GetPendingSalesbyNetsuiteWebAccount(Convert.ToInt32(webid), x.Id.ToString(), LastExecutionDate, null);


                        if (transactionList != null)
                        {
                            foreach (var item in transactionList.Items)
                            {
                                if (!string.IsNullOrEmpty(item.Document))
                                {
                                    int orderId = Convert.ToInt32(item.Orderid);
                                    var Order = GetOrderId(item.Id);
                                    if (x.tranId == null)
                                    {
                                        x.tranId = Order.tranId;

                                        if (Order.orderStatus?.id == "B")
                                            x.OrderStatus = OrderStatus.Processing;

                                        if (Order.orderStatus?.id == "G")
                                            x.OrderStatus = OrderStatus.Complete;
                                    }


                                    var invoiceNop = _invoice.GetInvoicesByCustomerOrderId(c.Id, x.Id);
                                    var Transaction = GetTransactionInvoice(item.Orderid, c.Parent).Items?.FirstOrDefault();

                                    if (invoiceNop == null)
                                    {
                                        if (Transaction != null)
                                        {
                                            Invoice invoice = new Invoice();
                                            //invoice.Id = Transaction.Orderid;
                                            invoice.CreatedDate = Transaction.Createddate;
                                            invoice.PONumber = Transaction.TranId;
                                            invoice.SaleOrderId = x.Id;
                                            invoice.CustomerId = c.Id;
                                            //invoice.Subtotal = Transaction.Foreigntotal;
                                            invoice.Total = Transaction.Foreigntotal;
                                            invoice.InvoiceNo = Transaction.TranId;
                                            invoice.Status = Transaction.Status;
                                            invoice.LastModifiedDate = Transaction.Lastmodifieddate;
                                            invoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                            try
                                            {
                                                _invoice.InsertInvoice(invoice);

                                                x.PaymentStatus = GetPaymentStatusFromNetsuite(Transaction.Status);

                                                _orderService.UpdateOrder(x);
                                            }
                                            catch (Exception ex)
                                            {
                                                _notificationService.ErrorNotification(ex.Message);
                                            _logger.Warning("ImportOrderError:: InsertInvoice Services:: Order Id" + item.Id +"  Transaction.TranId " + Transaction.TranId+ " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Transaction != null)
                                        {
                                            if (Transaction.Lastmodifieddate > invoiceNop.LastModifiedDate)
                                            {
                                                //invoice.Subtotal = Transaction.Foreigntotal;
                                                invoiceNop.Total = Transaction.Foreigntotal;
                                                invoiceNop.Status = Transaction.Status;
                                                invoiceNop.LastModifiedDate = Transaction.Lastmodifieddate;
                                                    if (invoiceNop.InvoiceNetSuiteId == 0)
                                                        invoiceNop.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                                    _invoice.UpdateOrder(invoiceNop);
                                            }
                                        }
                                    }

                                    if (Order.shipMethod != null)
                                    {
                                        if (Order.shipMethod?.id == _settingService.GetSetting("ShippingUpsGroup.id").Value ||
                                                Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAir.id").Value
                                            || Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value ||
                                                Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value ||
                                                Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDay.id").Value ||
                                                Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value ||
                                                Order.shipMethod?.id == _settingService.GetSetting("Shipping3DaySelect.id").Value)
                                        {
                                            if (Order.linkedTrackingNumbers != null)
                                            {
                                                string[] tracking = Order.linkedTrackingNumbers.Split(' ');
                                                foreach (var t in tracking)
                                                {
                                                    var trackingNumber = t;
                                                    var ExistTracking = x.Shipments.Where(r => r.TrackingNumber == trackingNumber).FirstOrDefault();

                                                    if (ExistTracking == null)
                                                    {
                                                        Shipment shipment = new Shipment
                                                        {
                                                            OrderId = x.Id,
                                                            TrackingNumber = trackingNumber,
                                                            TotalWeight = null,
                                                            ShippedDateUtc = null,
                                                            DeliveryDateUtc = null,
                                                            AdminComment = "",
                                                            CreatedOnUtc = DateTime.UtcNow
                                                        };
                                                        if (Order.shipDate != null)
                                                            shipment.ShippedDateUtc = Convert.ToDateTime(Order.shipDate);

                                                        foreach (var items in x.OrderItems)
                                                        {
                                                            ShipmentItem it = new ShipmentItem();

                                                            it.OrderItemId = items.Id;
                                                            it.Quantity = items.Quantity;

                                                            shipment.ShipmentItems.Add(it);
                                                        }

                                                        x.Shipments.Add(shipment);
                                                    }
                                                }


                                            }
                                            x.ShippingMethod = Order.shipMethod?.refName;
                                            x.ShippingRateComputationMethodSystemName = "Shipping.UPS";
                                        }
                                        //Pickup.PickupInStore
                                        if (Order.shipMethod?.id == _settingService.GetSetting("ShippingPickupInStore.id").Value)//"2691")
                                        {
                                            //validar la direccion del address
                                            x.ShippingMethod = "Pickup at Atlanta Office";
                                            x.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activepickuppointprovidersystemnames").Value;
                                        }
                                        //Shipping.NNDelivery
                                        if (Order.shipMethod?.id == "2706" || Order.shipMethod?.id == _settingService.GetSetting("ShippingNNDelivery.id").Value)// "2698")
                                        {
                                            x.ShippingMethod = "N&N Delivery";
                                            x.ShippingRateComputationMethodSystemName = "Shipping.NNDelivery";
                                            //validar la direccion del shipping
                                        }
                                        //Freight
                                        if (Order.shipMethod?.id == "1915")
                                        {
                                            x.ShippingMethod = Order.shipMethod?.refName;
                                        }

                                        x.ShippingMethod = Order.shipMethod?.refName;
                                        x.OrderShippingInclTax = Convert.ToDecimal(Order.shippingCost);
                                        if (Order.custbody10 != null)
                                        {
                                            x.ShippingMethod = Order.custbody10?.refName;
                                        }

                                        x.OrderStatusId = GetOrderStatus(Order.status?.id);

                                        if (Order.orderStatus?.id == "G")
                                            x.PaymentStatus = PaymentStatus.Paid;
                                        else
                                            x.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status?.id.ToString());


                                        _orderService.UpdateOrder(x);

                                    }
                                }
                            }
                        }
                    }
                }
                }
            }
            catch (Exception ex)
            {
                _notificationService.WarningNotification(ex.Message);
                _logger.Warning("ImportOrderError:: InsertInvoice Services:: company Id" + comp.Id + "  NesuiteId " + comp.NetsuiteId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

        }

        public void GetTransactionInfoOpenAndPending(Company comp, Customer customer, string LastExecutionDate=null)
        {
            try
            {
                //1. Get open sales order
                var transactionList = new TransactionListDto();
                if (comp?.NetsuiteId != null)
                {
                    // Get orders Pending for approval
                    transactionList = GetPendingSalesbyNetsuite(Convert.ToInt32(comp.NetsuiteId), LastExecutionDate);
                    if (transactionList != null)
                    {
                        if (transactionList.Items.Count > 0)
                        {
                            foreach (var item in transactionList.Items)
                            {
                                if (item.Document == "SO111350")
                                {

                                }

                                CreateOrderNopCommerceByNetsuite(customer, item);
                            }
                        }

                    }

                    // Get orders Pending Fulfillment 
                    var PendingFulfillmentOrderList = GetOpenSalesbyNetsuite(Convert.ToInt32(comp.NetsuiteId), LastExecutionDate);
                    if (PendingFulfillmentOrderList != null)
                    {
                        foreach (var item in PendingFulfillmentOrderList.Items)
                        {
                            CreateOrderNopCommerceByNetsuite(customer, item);
                        }
                    }

                    GetOrderFromNetsuitePendingAndBilled(comp.NetsuiteId, comp.Id, LastExecutionDate);
                }
                else
                {
                    var orderPendingList = _orderService.GetOrderByCustomer(customer.Id);

                    foreach (var x in orderPendingList)
                    {
                        var webid = _settingService.GetSetting("WebStoreCustomer.id").Value;
                        if (webid != null)
                            transactionList = GetPendingSalesbyNetsuiteWebAccount(Convert.ToInt32(webid), x.Id.ToString(), LastExecutionDate, null);


                        if (transactionList != null)
                        {
                            foreach (var item in transactionList.Items)
                            {
                                if (!string.IsNullOrEmpty(item.Document))
                                {
                                    int orderId = Convert.ToInt32(item.Orderid);
                                    var Order = GetOrderId(item.Id);
                                    if (x.tranId == null)
                                    {
                                        x.tranId = Order.tranId;

                                        if (Order.orderStatus?.id == "B")
                                            x.OrderStatus = OrderStatus.Processing;

                                        if (Order.orderStatus?.id == "G")
                                            x.OrderStatus = OrderStatus.Complete;
                                    }


                                    var invoiceNop = _invoice.GetInvoicesByCustomerOrderId(customer.Id, x.Id);
                                    var Transaction = GetTransactionInvoice(item.Orderid, customer.Parent).Items?.FirstOrDefault();

                                    if (invoiceNop == null)
                                    {
                                        if (Transaction != null)
                                        {
                                            Invoice invoice = new Invoice();
                                            //invoice.Id = Transaction.Orderid;
                                            invoice.CreatedDate = Transaction.Createddate;
                                            invoice.PONumber = Transaction.TranId;
                                            invoice.SaleOrderId = x.Id;
                                            invoice.CustomerId = customer.Id;
                                            //invoice.Subtotal = Transaction.Foreigntotal;
                                            invoice.Total = Transaction.Foreigntotal;
                                            invoice.InvoiceNo = Transaction.TranId;
                                            invoice.Status = Transaction.Status;
                                            invoice.LastModifiedDate = Transaction.Lastmodifieddate;
                                            invoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);
                                            try
                                            {
                                                _invoice.InsertInvoice(invoice);

                                                x.PaymentStatus = GetPaymentStatusFromNetsuite(Transaction.Status);

                                                _orderService.UpdateOrder(x);
                                            }
                                            catch (Exception ex)
                                            {
                                                _notificationService.ErrorNotification(ex.Message);
                                                _logger.Warning("ImportOrderError:: InsertInvoice Services orderPendingList:: company Id" + comp.Id + "  NesuiteId " + comp.NetsuiteId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);


                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Transaction != null)
                                        {
                                            if (Transaction.Lastmodifieddate > invoiceNop.LastModifiedDate)
                                            {
                                                //invoice.Subtotal = Transaction.Foreigntotal;
                                                invoiceNop.Total = Transaction.Foreigntotal;
                                                invoiceNop.Status = Transaction.Status;
                                                invoiceNop.LastModifiedDate = Transaction.Lastmodifieddate;
                                                if(invoiceNop.InvoiceNetSuiteId==0)
                                                    invoiceNop.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);
                                                _invoice.UpdateOrder(invoiceNop);
                                            }
                                        }
                                    }

                                    if (Order.shipMethod != null)
                                    {
                                        if (Order.shipMethod?.id == _settingService.GetSetting("ShippingUpsGroup.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAir.id").Value
                                           || Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDay.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("Shipping3DaySelect.id").Value)
                                        {
                                            if (Order.linkedTrackingNumbers != null)
                                            {
                                                string[] tracking = Order.linkedTrackingNumbers.Split(' ');
                                                foreach (var t in tracking)
                                                {
                                                    var trackingNumber = t;
                                                    var ExistTracking = x.Shipments.Where(r => r.TrackingNumber == trackingNumber).FirstOrDefault();

                                                    if (ExistTracking == null)
                                                    {
                                                        Shipment shipment = new Shipment
                                                        {
                                                            OrderId = x.Id,
                                                            TrackingNumber = trackingNumber,
                                                            TotalWeight = null,
                                                            ShippedDateUtc = null,
                                                            DeliveryDateUtc = null,
                                                            AdminComment = "",
                                                            CreatedOnUtc = DateTime.UtcNow
                                                        };
                                                        if (Order.shipDate != null)
                                                            shipment.ShippedDateUtc = Convert.ToDateTime(Order.shipDate);

                                                        foreach (var items in x.OrderItems)
                                                        {
                                                            ShipmentItem it = new ShipmentItem();

                                                            it.OrderItemId = items.Id;
                                                            it.Quantity = items.Quantity;

                                                            shipment.ShipmentItems.Add(it);
                                                        }

                                                        x.Shipments.Add(shipment);
                                                    }
                                                }


                                            }
                                            x.ShippingMethod = Order.shipMethod?.refName;
                                            x.ShippingRateComputationMethodSystemName = "Shipping.UPS";
                                        }
                                        //Pickup.PickupInStore
                                        if (Order.shipMethod?.id == _settingService.GetSetting("ShippingPickupInStore.id").Value)//"2691")
                                        {
                                            //validar la direccion del address
                                            x.ShippingMethod = "Pickup at Atlanta Office";
                                            x.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activepickuppointprovidersystemnames").Value;
                                        }
                                        //Shipping.NNDelivery
                                        if (Order.shipMethod?.id == "2706" || Order.shipMethod?.id == _settingService.GetSetting("ShippingNNDelivery.id").Value)// "2698")
                                        {
                                            x.ShippingMethod = "N&N Delivery";
                                            x.ShippingRateComputationMethodSystemName = "Shipping.NNDelivery";
                                            //validar la direccion del shipping
                                        }
                                        //Freight
                                        if (Order.shipMethod?.id == "1915")
                                        {
                                            x.ShippingMethod = Order.shipMethod?.refName;
                                        }

                                        x.ShippingMethod = Order.shipMethod?.refName;
                                        x.OrderShippingInclTax = Convert.ToDecimal(Order.shippingCost);
                                        if (Order.custbody10 != null)
                                        {
                                            x.ShippingMethod = Order.custbody10?.refName;
                                        }

                                        x.OrderStatusId = GetOrderStatus(Order.status?.id);

                                        if (Order.orderStatus?.id == "G")
                                            x.PaymentStatus = PaymentStatus.Paid;
                                        else
                                            x.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status?.id.ToString());


                                        _orderService.UpdateOrder(x);

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.WarningNotification(ex.Message);
                _logger.Warning("ImportOrderError:: InsertInvoice Services orderPendingList:: company Id" + comp.Id + "  NesuiteId " + comp.NetsuiteId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

        }

        public void GetWebStoreCustomerOrders(string LastExecutionDate)
        {
            var webid = _settingService.GetSetting("WebStoreCustomer.id").Value;

            GetOrderFromNetsuitePendingFulfillment(webid, LastExecutionDate);
            GetOrderFromNetsuitePendingAndBilled(webid, 0, LastExecutionDate);
        }

        public void GetTransactionInfoOpenAndPending(CompanyCustomerMapping comp, Customer customer, string LastExecutionDate = null)
        {
            try
            {
                //1. Get open sales order
                var transactionList = new TransactionListDto();
                if (comp.Company?.NetsuiteId != null)
                {
                    // Get orders Pending for approval
                    transactionList = GetPendingSalesbyNetsuite(Convert.ToInt32(comp.Company.NetsuiteId), LastExecutionDate);
                    if (transactionList != null)
                    {
                        if (transactionList.Items.Count > 0)
                        {
                            foreach (var item in transactionList.Items)
                            {
                                CreateOrderNopCommerceByNetsuite(customer, item);
                            }
                        }

                    }

                    // Get orders Pending Fulfillment 
                    var PendingFulfillmentOrderList = GetOpenSalesbyNetsuite(Convert.ToInt32(comp.Company.NetsuiteId), LastExecutionDate);
                    if (PendingFulfillmentOrderList != null)
                    {
                        foreach (var item in PendingFulfillmentOrderList.Items)
                        {
                            CreateOrderNopCommerceByNetsuite(comp.Customer, item);
                        }
                    }

                    GetOrderFromNetsuitePendingAndBilled(comp.Company.NetsuiteId, comp.Company.Id, LastExecutionDate);
                }
                else
                {
                    var orderPendingList = _orderService.GetOrderByCustomer(customer.Id);

                    foreach (var x in orderPendingList)
                    {
                        var webid = _settingService.GetSetting("WebStoreCustomer.id").Value;
                        if (webid != null)
                            transactionList = GetPendingSalesbyNetsuiteWebAccount(Convert.ToInt32(webid), x.Id.ToString(), LastExecutionDate, null);


                        if (transactionList != null)
                        {
                            foreach (var item in transactionList.Items)
                            {
                                if (!string.IsNullOrEmpty(item.Document))
                                {
                                    int orderId = Convert.ToInt32(item.Orderid);
                                    var Order = GetOrderId(item.Id);
                                    if (x.tranId == null)
                                    {
                                        x.tranId = Order.tranId;

                                        if (Order.orderStatus?.id == "B")
                                            x.OrderStatus = OrderStatus.Processing;

                                        if (Order.orderStatus?.id == "G")
                                            x.OrderStatus = OrderStatus.Complete;
                                    }


                                    var invoiceNop = _invoice.GetInvoicesByCustomerOrderId(customer.Id, x.Id);
                                    var Transaction = GetTransactionInvoice(item.Orderid, customer.Parent).Items?.FirstOrDefault();

                                    if (invoiceNop == null)
                                    {
                                        if (Transaction != null)
                                        {
                                            Invoice invoice = new Invoice();
                                            //invoice.Id = Transaction.Orderid;
                                            invoice.CreatedDate = Transaction.Createddate;
                                            invoice.PONumber = Transaction.TranId;
                                            invoice.SaleOrderId = x.Id;
                                            invoice.CustomerId = customer.Id;
                                            //invoice.Subtotal = Transaction.Foreigntotal;
                                            invoice.Total = Transaction.Foreigntotal;
                                            invoice.InvoiceNo = Transaction.TranId;
                                            invoice.Status = Transaction.Status;
                                            invoice.LastModifiedDate = Transaction.Lastmodifieddate;
                                            invoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                            try
                                            {
                                                _invoice.InsertInvoice(invoice);

                                                x.PaymentStatus = GetPaymentStatusFromNetsuite(Transaction.Status);

                                                _orderService.UpdateOrder(x);
                                            }
                                            catch (Exception ex)
                                            {
                                                _notificationService.ErrorNotification(ex.Message);
                                                _logger.Warning("ImportOrderError:: InsertInvoice Services orderPendingList:: company Id" + comp.CompanyId  + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (Transaction != null)
                                        {
                                            if (Transaction.Lastmodifieddate > invoiceNop.LastModifiedDate)
                                            {
                                                //invoice.Subtotal = Transaction.Foreigntotal;
                                                invoiceNop.Total = Transaction.Foreigntotal;
                                                invoiceNop.Status = Transaction.Status;
                                                invoiceNop.LastModifiedDate = Transaction.Lastmodifieddate;
                                                if (invoiceNop.InvoiceNetSuiteId == 0)
                                                    invoiceNop.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                                _invoice.UpdateOrder(invoiceNop);
                                            }
                                        }
                                    }

                                    if (Order.shipMethod != null)
                                    {
                                        if (Order.shipMethod?.id == _settingService.GetSetting("ShippingUpsGroup.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAir.id").Value
                                           || Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDay.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value ||
                                              Order.shipMethod?.id == _settingService.GetSetting("Shipping3DaySelect.id").Value)
                                        {
                                            if (Order.linkedTrackingNumbers != null)
                                            {
                                                string[] tracking = Order.linkedTrackingNumbers.Split(' ');
                                                foreach (var t in tracking)
                                                {
                                                    var trackingNumber = t;
                                                    var ExistTracking = x.Shipments.Where(r => r.TrackingNumber == trackingNumber).FirstOrDefault();

                                                    if (ExistTracking == null)
                                                    {
                                                        Shipment shipment = new Shipment
                                                        {
                                                            OrderId = x.Id,
                                                            TrackingNumber = trackingNumber,
                                                            TotalWeight = null,
                                                            ShippedDateUtc = null,
                                                            DeliveryDateUtc = null,
                                                            AdminComment = "",
                                                            CreatedOnUtc = DateTime.UtcNow
                                                        };
                                                        if (Order.shipDate != null)
                                                            shipment.ShippedDateUtc = Convert.ToDateTime(Order.shipDate);

                                                        foreach (var items in x.OrderItems)
                                                        {
                                                            ShipmentItem it = new ShipmentItem();

                                                            it.OrderItemId = items.Id;
                                                            it.Quantity = items.Quantity;

                                                            shipment.ShipmentItems.Add(it);
                                                        }

                                                        x.Shipments.Add(shipment);
                                                    }
                                                }


                                            }
                                            x.ShippingMethod = Order.shipMethod?.refName;
                                            x.ShippingRateComputationMethodSystemName = "Shipping.UPS";
                                        }
                                        //Pickup.PickupInStore
                                        if (Order.shipMethod?.id == _settingService.GetSetting("ShippingPickupInStore.id").Value)//"2691")
                                        {
                                            //validar la direccion del address
                                            x.ShippingMethod = "Pickup at Atlanta Office";
                                            x.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activepickuppointprovidersystemnames").Value;
                                        }
                                        //Shipping.NNDelivery
                                        if (Order.shipMethod?.id == "2706" || Order.shipMethod?.id == _settingService.GetSetting("ShippingNNDelivery.id").Value)// "2698")
                                        {
                                            x.ShippingMethod = "N&N Delivery";
                                            x.ShippingRateComputationMethodSystemName = "Shipping.NNDelivery";
                                            //validar la direccion del shipping
                                        }
                                        //Freight
                                        if (Order.shipMethod?.id == "1915")
                                        {
                                            x.ShippingMethod = Order.shipMethod?.refName;
                                        }

                                        x.ShippingMethod = Order.shipMethod?.refName;
                                        x.OrderShippingInclTax = Convert.ToDecimal(Order.shippingCost);
                                        if (Order.custbody10 != null)
                                        {
                                            x.ShippingMethod = Order.custbody10?.refName;
                                        }

                                        x.OrderStatusId = GetOrderStatus(Order.status?.id);

                                        if (Order.orderStatus?.id == "G")
                                            x.PaymentStatus = PaymentStatus.Paid;
                                        else
                                            x.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status?.id.ToString());


                                        _orderService.UpdateOrder(x);

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.WarningNotification(ex.Message);
                _logger.Warning("ImportOrderError:: InsertInvoice Services OpenAndPending:: company Id" + comp.Id + "  customer " + customer.Id + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

        }
        #endregion

        #region Private Order

        #region Create Orders
        public void InsertOrder(OrderDto orderDto, List<OrderItemDto> orderItemList, Customer customer, TransactionDto item=null)
        {
            if (orderDto == null)
                throw new ArgumentNullException(nameof(orderDto));

            var store = _storeContext.CurrentStore;
            var ExistOrder = _orderService.GetOrderByNetsuiteId(orderDto.tranId);
            if (ExistOrder != null)
            {
            
            }
            else
			{
				var order = new Nop.Core.Domain.Orders.Order();
				order.OrderGuid = Guid.NewGuid();
				order.StoreId = store.Id;
				order.CustomerId = customer.Id;
				order.Customer = customer;
				var CompanyIngo = customer.Companies.Where(r => r.NetsuiteId == orderDto.entity.id).FirstOrDefault();

				var AddressIngoBilling = new CompanyAddresses();
				var AddressIngoShipping = new CompanyAddresses();

				if (CompanyIngo != null)
				{
					AddressIngoBilling = CompanyIngo.CompanyAddressMappings.Where(r => r.IsBilling).FirstOrDefault();
					AddressIngoShipping = CompanyIngo.CompanyAddressMappings.Where(r => r.IsShipping).FirstOrDefault();
				}

				if (orderDto.billAddressList != null)
				{
					AddressIngoBilling = CompanyIngo.CompanyAddressMappings.Where(r => r.Address.NetsuitId == Convert.ToInt32(orderDto.billAddressList?.id)).FirstOrDefault();
					AddressIngoShipping = CompanyIngo.CompanyAddressMappings.Where(r => r.Address.NetsuitId == Convert.ToInt32(orderDto.shipAddressList?.id)).FirstOrDefault();
				}

				if (AddressIngoShipping != null)
				{
					if (AddressIngoShipping.Address != null)
					{
						order.BillingAddressId = AddressIngoShipping.AddressId;
						order.ShippingAddressId = AddressIngoShipping.AddressId;

						order.BillingAddress = AddressIngoShipping.Address;
						order.ShippingAddress = AddressIngoShipping.Address;
					}
					if (AddressIngoBilling == null)
					{
						if (AddressIngoBilling?.Address == null)
						{
							var ship = orderDto.shipAddressList?.id;
							order.ShippingAddressId = Convert.ToInt32(ship);
							order.BillingAddressId = Convert.ToInt32(ship);
						}
					}
					else
					{
						AddressIngoBilling = CompanyIngo.CompanyAddressMappings.FirstOrDefault();
					}
				}
				else
				{
					if (AddressIngoBilling == null)
						AddressIngoBilling = CompanyIngo.CompanyAddressMappings.FirstOrDefault();
					if (AddressIngoShipping == null)
						AddressIngoShipping = CompanyIngo.CompanyAddressMappings.FirstOrDefault();

					if (AddressIngoShipping != null)
					{
						order.BillingAddressId = AddressIngoShipping.AddressId;
						order.ShippingAddressId = AddressIngoShipping.AddressId;
					}
					else
					{
						order.BillingAddressId = AddressIngoBilling.AddressId;
						order.ShippingAddressId = AddressIngoBilling.AddressId;
					}
				}

				order.OrderStatusId = GetOrderStatus(orderDto.status?.id);

				order.ShippingStatusId = orderDto.shipComplete ? (int)ShippingStatus.Delivered : (int)ShippingStatus.NotYetShipped;
				order.PaymentMethodSystemName = "Payments.AuthorizeNet";
				order.CustomerCurrencyCode = _workContext.WorkingCurrency.CurrencyCode;
				order.CurrencyRate = orderDto.exchangeRate != 0 ? orderDto.exchangeRate : _workContext.WorkingCurrency.Rate;
				
                if (orderDto.orderStatus?.id == "G")
					order.PaymentStatus = PaymentStatus.Paid;
				else
					order.PaymentStatus = GetPaymentStatusFromNetsuite(orderDto.custbody_website_order_status?.id.ToString());

				order.VatNumber = null;
				order.OrderSubtotalInclTax = Convert.ToDecimal(orderDto.subtotal);
				order.OrderSubtotalExclTax = Convert.ToDecimal(orderDto.subtotal);

				order.OrderSubTotalDiscountInclTax = 0;
				order.OrderSubTotalDiscountExclTax = 0;
				order.OrderShippingInclTax = Convert.ToDecimal(orderDto.shippingCost);
				order.OrderShippingExclTax = Convert.ToDecimal(orderDto.shippingCost);
				order.PaymentMethodAdditionalFeeInclTax = 0;
				order.PaymentMethodAdditionalFeeExclTax = 0;
				order.TaxRates = "0:0;";
				order.OrderTotal = Convert.ToDecimal(orderDto.total);

				order.OrderTax = Convert.ToDecimal(orderDto.total - orderDto.shippingCost - orderDto.subtotal);
				order.OrderDiscount = 0;


				order.RefundedAmount = 0;

				order.CheckoutAttributeDescription = "0";
				order.CheckoutAttributesXml = "0";
				order.CustomerIp = "0";
				order.AllowStoringCreditCardNumber = false;
				order.CardType = "0";
				order.CardName = "0";
				order.MaskedCreditCardNumber = "0";
				order.CardCvv2 = "0";
				order.CardExpirationMonth = "0";
				order.CardExpirationYear = "0";
				order.AuthorizationTransactionId = "0";
				order.AuthorizationTransactionCode = "0";
				order.AuthorizationTransactionResult = "0";
				order.CaptureTransactionId = "0";
				order.CaptureTransactionResult = "0";
				order.SubscriptionTransactionId = "0";
				order.PaidDateUtc = Convert.ToDateTime(orderDto.tranDate);
				order.ShippingMethod = orderDto.shipMethod?.refName;
				order.PickupInStore = true;

				if (orderDto.shipMethod.id == _settingService.GetSetting("ShippingUpsGroup.id").Value)// "UPS Ground")
				{
					order.ShippingMethod = _settingService.GetSetting("ShippingUpsGroup.Name").Value;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("ShippingSelectedName.UPS").Value;  //"Shipping.UPS"
				}

				if (orderDto.shipMethod.id == _settingService.GetSetting("ShippingNextDayAir.id").Value)// "UPS Ground")
				{
					order.ShippingMethod = _settingService.GetSetting("ShippingNextDayAir.Name").Value;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("ShippingSelectedName.UPS").Value;  //"Shipping.UPS"
				}

				if (orderDto.shipMethod.id == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value)// "UPS Ground")
				{
					order.ShippingMethod = _settingService.GetSetting("ShippingNextDayAirEarlyAm.Name").Value;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("ShippingSelectedName.UPS").Value;  //"Shipping.UPS"
				}

				if (orderDto.shipMethod.id == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value)// "UPS Ground")
				{
					order.ShippingMethod = _settingService.GetSetting("ShippingNextDayAirSaver.Name").Value;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("ShippingSelectedName.UPS").Value;  //"Shipping.UPS"
				}
				if (orderDto.shipMethod.id == _settingService.GetSetting("ShippingSecondDay.id").Value)// "UPS Ground")
				{
					order.ShippingMethod = _settingService.GetSetting("ShippingSecondDay.Name").Value;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("ShippingSelectedName.UPS").Value;  //"Shipping.UPS"
				}

				if (orderDto.shipMethod.id == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value)// "UPS Ground")
				{
					order.ShippingMethod = _settingService.GetSetting("ShippingSecondDayAirAm.Name").Value;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("ShippingSelectedName.UPS").Value;  //"Shipping.UPS"
				}
				if (orderDto.shipMethod.id == _settingService.GetSetting("Shipping3DaySelect.id").Value)// "UPS Ground")
				{
					order.ShippingMethod = _settingService.GetSetting("Shipping3DaySelect.Name").Value;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("ShippingSelectedName.UPS").Value;  //"Shipping.UPS"
				}

				if (orderDto.shipMethod.id == _settingService.GetSetting("shippingpickupinstore.id").Value)// shippingpickupinstore
				{
					order.PickupInStore = true;
					order.ShippingMethod = orderDto.shipMethod?.refName;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activepickuppointprovidersystemnames").Value;  //"Shipping.UPS"
				}
				if (orderDto.shipMethod.id == _settingService.GetSetting("shippingnndelivery.id").Value)
				{
					order.ShippingMethod = orderDto.shipMethod?.refName;
					order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activeshippingratecomputationmethodsystemnames").Value;  //"Shipping.UPS"
					if (orderDto.custbody_delivery_routes_atl != null)
					{
						order.ShippingNNDeliverySend = orderDto.custbody_delivery_routes_atl?.refName;
						order.IdShippingNNDeliverySend = Convert.ToInt32(orderDto.custbody_delivery_routes_atl?.id);
					}
					if (orderDto.custbody_delivery_routes_cinci != null)
					{
						order.ShippingNNDeliverySend = orderDto.custbody_delivery_routes_cinci?.refName;
						order.IdShippingNNDeliverySend = Convert.ToInt32(orderDto.custbody_delivery_routes_cinci?.id);
					}
					if (orderDto.custbody_delivery_routes_nash != null)
					{
						order.ShippingNNDeliverySend = orderDto.custbody_delivery_routes_nash?.refName;
						order.IdShippingNNDeliverySend = Convert.ToInt32(orderDto.custbody_delivery_routes_nash?.id);
					}
				}

				order.Deleted = false;
				order.CreatedOnUtc = Convert.ToDateTime(orderDto.tranDate);
				
				order.CompanyId = Convert.ToInt32(CompanyIngo.Id);
				order.tranId = orderDto.tranId;
				order.NNDeliveryDate = orderDto.custbody34;

                if (orderDto.source?.refName?.Contains("REST") == true)
                {
                    order.Source = orderDto.source.refName;
                }


                //save item
                UpdateOrderItemsFromNetsuite(orderItemList, order);
				_orderService.InsertOrder(order);

				if (order.Id > 0)
				{
                    
                    var companyId = _companyService.GetCompanyById(Convert.ToInt32(order.CompanyId));

					var orderId = order.Id;
					if (orderDto.id != null)
						orderId = Convert.ToInt32(orderDto.id);

					if (item != null)
					{
                        CreateUpdateInvoice(item, orderId, order, Convert.ToInt32(companyId.NetsuiteId));

                        GetTransactionOrderStatus(order, item);

                    }

                    order.CustomOrderNumber = order.Id.ToString();
                    _orderService.UpdateOrder(order);

                }

			}

		}

		private void UpdateOrderItemsFromNetsuite(List<OrderItemDto> orderItemList, Nop.Core.Domain.Orders.Order order)
		{
			foreach (var orderItem in orderItemList)
			{
				if (orderItem != null)
				{
					var productExist = _productService.GetProductsByNetsuiteItem(orderItem.item.id);

					if (productExist != null)
					{

                        var productItem = order.OrderItems.Where(r=>r.ProductId==productExist.Id).FirstOrDefault();
						if (productItem != null)
						{
                            productItem.UnitPriceInclTax = Convert.ToDecimal(orderItem.printItems);
                            productItem.UnitPriceExclTax = Convert.ToDecimal(orderItem.rate);
                            productItem.PriceInclTax = Convert.ToDecimal(orderItem.amount);
                            productItem.PriceExclTax = Convert.ToDecimal(orderItem.amount);
                            productItem.OriginalProductCost = Convert.ToDecimal(orderItem.costEstimate);
                            productItem.AttributeDescription = orderItem.description;
                            productItem.Quantity = Convert.ToInt32(orderItem.quantity);
						}
						else
						{
                            var newOrderItem = new OrderItem
                            {
                                OrderItemGuid = Guid.NewGuid(),
                                Order = order,
                                //OrderId = order.Id,
                                Product = productExist != null ? productExist : null,
                                ProductId = productExist != null ? productExist.Id : int.Parse(orderItem.item.id),
                                UnitPriceInclTax = Convert.ToDecimal(orderItem.printItems),
                                UnitPriceExclTax = Convert.ToDecimal(orderItem.rate),
                                PriceInclTax = Convert.ToDecimal(orderItem.amount),
                                PriceExclTax = Convert.ToDecimal(orderItem.amount),
                                OriginalProductCost = Convert.ToDecimal(orderItem.costEstimate),
                                AttributeDescription = orderItem.description,
                                AttributesXml = null,
                                Quantity = Convert.ToInt32(orderItem.quantity),
                                DiscountAmountInclTax = 0,
                                DiscountAmountExclTax = 0,
                                DownloadCount = 0,
                                IsDownloadActivated = false,
                                LicenseDownloadId = 0,
                                ItemWeight = 0,
                                RentalStartDateUtc = null,
                                RentalEndDateUtc = null
                            };
                            order.OrderItems.Add(newOrderItem);
                        }
                        
                        
					}
					else
					{

						if (!string.IsNullOrEmpty(orderItem.item.id))
						{
							var product = _productServiceNetsuite.CreateProductbyNetsuite(orderItem);

							var newOrderItem = new OrderItem
							{
								OrderItemGuid = Guid.NewGuid(),
								Order = order,
								//OrderId = order.Id,
								Product = product != null ? product : null,
								ProductId = product != null ? product.Id : int.Parse(orderItem.item.id),
								UnitPriceInclTax = Convert.ToDecimal(orderItem.printItems),
								UnitPriceExclTax = Convert.ToDecimal(orderItem.rate),
								PriceInclTax = Convert.ToDecimal(orderItem.amount),
								PriceExclTax = Convert.ToDecimal(orderItem.amount),
								OriginalProductCost = Convert.ToDecimal(orderItem.costEstimate),
								AttributeDescription = orderItem.description,
								AttributesXml = null,
								Quantity = Convert.ToInt32(orderItem.quantity),
								DiscountAmountInclTax = 0,
								DiscountAmountExclTax = 0,
								DownloadCount = 0,
								IsDownloadActivated = false,
								LicenseDownloadId = 0,
								ItemWeight = 0,
								RentalStartDateUtc = null,
								RentalEndDateUtc = null
							};
							order.OrderItems.Add(newOrderItem);
						}
					}

				}

			}
		}

		private void CreateOrderbyNetSuite(int Id, Customer customer = null, TransactionDto item=null)
        {
            var orderDto = GetOrderId(Id);
            if (orderDto != null)
			{
				List<OrderItemDto> listOrderItemDto = GetOrderItems(Id);

				InsertOrder(orderDto, listOrderItemDto, customer, item);

			}
		}

		private List<OrderItemDto> GetOrderItems(int Id)
		{
			var orderItemList = GetItemOrderListId(Id);
			var listOrderItemDto = new List<OrderItemDto>();
			if (orderItemList != null)
			{
				foreach (var orderItemListItem in orderItemList.items)
				{
					var link = orderItemListItem.links.FirstOrDefault();
					int indexItem = link.href.IndexOf("/item/");
					var linkItemId = link.href.Substring(indexItem + 6);

					var orderItem = GetItemOrderId(Id, linkItemId);
					listOrderItemDto.Add(orderItem);
				}
			}

			return listOrderItemDto;
		}

		private void CreateOrderNopCommerceByNetsuite(Customer customer, TransactionDto item)
        {
            try
            {
                if (!string.IsNullOrEmpty(item.Document))
                {
                    if (item.Document == "SO111453")
                    {

                    }
                    int orderId = Convert.ToInt32(item.Orderid);

                    var isOrder = false;
                    var OrderIdNc = "";

                    if (item.OrderNopId != null)
                    {
                        OrderIdNc = item.OrderNopId;

                        if (TestMode)
                        {
                            string[] orderIdSplit = item.OrderNopId.Split("NCTest_");
                            if (orderIdSplit.Count() > 1)
                            {
                                OrderIdNc = orderIdSplit[1];
                                var order = _orderService.GetOrderById(Convert.ToInt32(OrderIdNc));

                                if (order == null)
                                {
                                    var ExistOrder = _orderService.GetOrderByNetsuiteId(item.Document);
                                    if (ExistOrder == null)
                                    {
                                        CreateOrderbyNetSuite(orderId, customer, item);
                                        order = _orderService.GetOrderByNetsuiteId(item.Document);
                                    }
                                }
                                else
                                {
                                    isOrder = true;
                                }
                            }

                        }
                        else
                        {
                            string[] orderIdSplit = item.OrderNopId.Split("NC_");
                            if (orderIdSplit.Count() > 1)
                            {
                                OrderIdNc = orderIdSplit[1];
                                var order = _orderService.GetOrderById(Convert.ToInt32(OrderIdNc));

                                if (order == null)
                                {
                                    var ExistOrder = _orderService.GetOrderByNetsuiteId(item.Document);
                                    if (ExistOrder == null)
                                    {
                                        CreateOrderbyNetSuite(orderId, customer, item);
                                        order = _orderService.GetOrderByNetsuiteId(item.Document);
                                    }
                                    else
                                    {
                                        isOrder = true;
                                    }
                                }
                                else
                                {
                                    isOrder = true;
                                }

                            }

                        }

                        GetOrderitemFulfillment(item, orderId, isOrder, OrderIdNc);
                    }
                    else
                    {
                        var ExistOrder = _orderService.GetOrderByNetsuiteId(item.Document);
                        if (ExistOrder == null)
                        {
                            CreateOrderbyNetSuite(orderId, customer, item);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                // _logger.Warning("ImportOrder Error: CreateOrderNopCommerceByNetsuite ", ex);
            }
        }

        private void GetOrderitemFulfillment(TransactionDto item, int orderId, bool isOrder, string OrderIdNc)
        {
            if (isOrder)
            {
                var order = _orderService.GetOrderById(Convert.ToInt32(OrderIdNc));

                if (order == null)
                    order = _orderService.GetOrderByNetsuiteId(item.Document);

                if (order != null)
                {
                    var CompanyId = 0;
                    var company = new Company();
                    if (order.CompanyId != null)
                    {
                        company = _companyService.GetCompanyById(Convert.ToInt32(order.CompanyId));
                        
                    }

                    if(company!=null)
                        CreateUpdateInvoice(item, orderId, order, Convert.ToInt32(company.Id));

                    var ItemFullFitmentStatus = GetTransactionOrderStatus(orderId.ToString());

                    if (ItemFullFitmentStatus != null)
                    {
                        //foreach (var x in ItemFullFitmentStatus.items)
                        //{
                        if (ItemFullFitmentStatus.items.Count() > 0)
                        {
                            ItemFullfitmentDto getInfoStatusShipped = GetitemFulfillmentNetsuite(ItemFullFitmentStatus.items?.FirstOrDefault().transaction?.ToString());
                            if (getInfoStatusShipped != null)
							{
								GetShippingStatusValue(order, getInfoStatusShipped.shipStatus.refName);
							}
						}
                        if (order.Id > 0)
                            _orderService.UpdateOrder(order);
                    }
                }
            }
        }

		private static void GetShippingStatusValue(Core.Domain.Orders.Order order, string shipStatus)
		{
			if (shipStatus.ToLower() == ShippingStatus.Picked.ToString().ToLower())
			{
				order.ShippingStatus = ShippingStatus.Picked;
			}


			if (shipStatus.ToLower() == ShippingStatus.Packed.ToString().ToLower())
			{
				if (order.ShippingRateComputationMethodSystemName == "Shipping.UPS")
				{
					order.ShippingStatus = ShippingStatus.ReadyforUPSPickup;
				}
				if (order.ShippingRateComputationMethodSystemName.Contains("Shipping.NNDelivery"))
				{
					order.ShippingStatus = ShippingStatus.ReadyforDelivery;
				}
				if (order.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
				{
					order.ShippingStatus = ShippingStatus.ReadyforPickup;
				}

			}

			if (shipStatus.ToLower() == ShippingStatus.Delivered.ToString().ToLower())
				order.ShippingStatus = ShippingStatus.Delivered;

			if (shipStatus.ToLower() == ShippingStatus.Shipped.ToString().ToLower())
				order.ShippingStatus = ShippingStatus.Shipped;
		}

		private void CreateUpdateInvoice(TransactionDto item, int orderId, Nop.Core.Domain.Orders.Order order, int CompanyId)
        {

            try
            {
                if (CompanyId != null )
				{
					var OrderNopId = item.OrderNopId;
					var numero = 0;
					var isNumber = int.TryParse(item.OrderNopId, out numero);
					if (isNumber)
					{
						if (item.OrderNopId == null)
						{
							order = _orderService.GetOrderByNetsuiteId(item.Document);
							if (order != null)
								OrderNopId = order.Id.ToString();
						}
					}
					else
						OrderNopId = order.Id.ToString();

					var webStoreCustomerid = _settingService.GetSetting("WebStoreCustomer.id").Value;

					if (webStoreCustomerid != null)
					{
						var TransactionCashSales = GetTransactionCashSales(OrderNopId, Convert.ToInt32(item.Customerid));

						if (TransactionCashSales != null)
						{
							if (TransactionCashSales != null)
							{
								var trans = TransactionCashSales.Items.FirstOrDefault();
								if (trans != null)
									UpdateOrderFromNetsuiteNopcommerce(item, order, CompanyId, trans);
							}
							else
							{
								TransactionCashSales = GetTransactionCashSalesOrderId(order.Id.ToString(), Convert.ToInt32(item.Customerid));

								if (TransactionCashSales != null)
								{
									var trans = TransactionCashSales.Items.FirstOrDefault();
									if (trans != null)
										UpdateOrderFromNetsuiteNopcommerce(item, order, CompanyId, trans);
								}
							}
						}
					}



					var invoiceNop = _invoice.GetInvoicesByCustomerOrderId(CompanyId, order.Id);

					var Transaction = GetTransactionInvoice(OrderNopId, Convert.ToInt32(item.Customerid));

					if (invoiceNop == null)
					{

						if (Transaction != null)
						{
							var trans = Transaction.Items.FirstOrDefault();
							if (trans != null)
								UpdateOrderFromNetsuiteNopcommerce(item, order, CompanyId, trans);
						}
						else
						{
							Transaction = GetTransactionInvoiceOrderId(order.Id.ToString(), Convert.ToInt32(item.Customerid));

							if (Transaction != null)
							{
								var trans = Transaction.Items.FirstOrDefault();
								if (trans != null)
									UpdateOrderFromNetsuiteNopcommerce(item, order, CompanyId, trans);
							}
						}
					}
					else
					{
						if (Transaction != null)
						{
							var Trans = Transaction.Items.FirstOrDefault();
							if (Trans != null)
							{
								var InvoiceInfo = GetInvoiceFromNetsuite(Trans.Orderid.ToString());
								if (InvoiceInfo != null)
								{
									if (InvoiceInfo.amountRemaining <= invoiceNop.AmountDue)
									{
										invoiceNop.StatusPaymentNP = true;
										invoiceNop.AmountDue = InvoiceInfo.amountRemaining;
									}
									//else
									//{
									//    invoiceNop.StatusPaymentNP = false;
									//    //getInvoice.AmountDue = InvoiceInfo.amountRemaining; 
									//}
								}

								if (Trans.Lastmodifieddate > invoiceNop.LastModifiedDate)
								{
									//invoice.Subtotal = Transaction.Foreigntotal;
									invoiceNop.Total = Trans.Foreigntotal;
									invoiceNop.Status = Trans.Status;
									invoiceNop.LastModifiedDate = Trans.Lastmodifieddate;

								}
								else
								{
									if (Trans.Status == "B")
									{
										invoiceNop.StatusPaymentNP = true;
										invoiceNop.AmountDue = InvoiceInfo.amountRemaining;
										invoiceNop.PaymentStatusId = (int)PaymentStatus.Paid;
									}
								}

                                if (invoiceNop.InvoiceNetSuiteId == 0)
                                    invoiceNop.InvoiceNetSuiteId = Convert.ToInt32(Trans.Orderid);

                                _invoice.UpdateOrder(invoiceNop);


								//if(InvoiceInfo.status.id== "Paid In Full")
								//{
								//    order.PaymentStatus = PaymentStatus.Paid;
								//    _orderService.UpdateOrder(order);
								//}
							}
						}

					}

					UpdateShippingMethodFromNetsuite(orderId, order);
				}
			}
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                //_logger.Warning("ImportOrder Error: UpdateOrderFromNetsuiteNopcommerce ", ex);
            }

        }

		private void UpdateShippingMethodFromNetsuite(int orderId, Core.Domain.Orders.Order order)
		{
			var Order = GetOrderId(orderId);
			if (Order != null)
			{
				if (Order.shipMethod != null)
				{
					GetShippingMethod(order, Order.shipMethod?.id, Order.shipMethod?.refName);

					if (Order.custbody10 != null)
					{
						order.ShippingMethod = Order.custbody10?.refName;

					}

					ValidateShippingMethodUps(order, Order);
					ValidateShippingMethodStorePickUpNNDelivery(order, Order);
					//if (Order.status?.id != "E")
					order.OrderStatusId = GetOrderStatus(Order.status?.id);
					//if(Order.custbody_website_order_statu==null)

					if (Order.orderStatus?.id == "G")
						order.PaymentStatus = PaymentStatus.Paid;
					else
						 if (order.PaymentStatus != PaymentStatus.Paid)
						order.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status?.id.ToString());

					order.NNDeliveryDate = Order.custbody34;


					if (order.Id != 0)
						_orderService.UpdateOrder(order);
				}


			}
		}

		private void ValidateShippingMethodStorePickUpNNDelivery(Core.Domain.Orders.Order order, OrderDto Order)
		{
			//Pickup.PickupInStore
			if (Order.shipMethod?.id == _settingService.GetSetting("ShippingPickupInStore.id").Value)
			{
				order.ShippingMethod = Order.shipMethod?.refName;
				order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activepickuppointprovidersystemnames").Value;  //"Shipping.UPS"
				order.PickupInStore = true;
			}
			//Shipping.NNDelivery
			if (Order.shipMethod?.id == "2706" || Order.shipMethod?.id == _settingService.GetSetting("shippingnndelivery.id").Value)
			{
				order.ShippingMethod = Order.shipMethod?.refName;
				order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activeshippingratecomputationmethodsystemnames").Value;  //"Shipping.UPS"
				if (Order.custbody_delivery_routes_atl != null)
				{
					order.ShippingNNDeliverySend = Order.custbody_delivery_routes_atl?.refName;
					order.IdShippingNNDeliverySend = Convert.ToInt32(Order.custbody_delivery_routes_atl?.id);
				}
				if (Order.custbody_delivery_routes_cinci != null)
				{
					order.ShippingNNDeliverySend = Order.custbody_delivery_routes_cinci?.refName;
					order.IdShippingNNDeliverySend = Convert.ToInt32(Order.custbody_delivery_routes_cinci?.id);
				}
				if (Order.custbody_delivery_routes_nash != null)
				{
					order.ShippingNNDeliverySend = Order.custbody_delivery_routes_nash?.refName;
					order.IdShippingNNDeliverySend = Convert.ToInt32(Order.custbody_delivery_routes_nash?.id);
				}
			}
		}

		private void ValidateShippingMethodUps(Core.Domain.Orders.Order order, OrderDto Order)
		{
			if (Order.shipMethod?.id == _settingService.GetSetting("ShippingUpsGroup.id").Value ||
											Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAir.id").Value
										 || Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value ||
											Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value ||
											Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDay.id").Value ||
											Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value ||
											Order.shipMethod?.id == _settingService.GetSetting("Shipping3DaySelect.id").Value)
			{
				if (Order.linkedTrackingNumbers != null)
				{
					string[] tracking = Order.linkedTrackingNumbers.Split(' ');
					foreach (var t in tracking)
					{
						var trackingNumber = t;
						var ExistTracking = order.Shipments.Where(r => r.TrackingNumber == trackingNumber).FirstOrDefault();

						if (ExistTracking == null)
						{
							Shipment shipment = new Shipment
							{
								OrderId = order.Id,
								TrackingNumber = trackingNumber,
								TotalWeight = null,
								ShippedDateUtc = null,
								DeliveryDateUtc = null,
								AdminComment = "",
								CreatedOnUtc = DateTime.UtcNow,
							};
							if (Order.shipDate != null)
								shipment.ShippedDateUtc = Convert.ToDateTime(Order.shipDate);

							foreach (var items in order.OrderItems)
							{
								ShipmentItem it = new ShipmentItem();

								it.OrderItemId = items.Id;
								it.Quantity = items.Quantity;

								shipment.ShipmentItems.Add(it);
							}

							order.Shipments.Add(shipment);
						}
					}
				}
				order.ShippingMethod = Order.shipMethod?.refName;
				order.ShippingRateComputationMethodSystemName = "Shipping.UPS";
			}
		}

		private void GetShippingMethod(Core.Domain.Orders.Order order, string shipMethodId, string shipMethodrefName)
		{
			if (shipMethodId == _settingService.GetSetting("ShippingPickupInStore.id").Value)
			{
                order.ShippingMethod = shipMethodrefName;//Order.shipMethod?.refName;
				order.ShippingRateComputationMethodSystemName = _settingService.GetSetting("shippingsettings.activepickuppointprovidersystemnames").Value;  //"Shipping.UPS"
				order.PickupInStore = true;
			}

			if (shipMethodId  == "2518")
			{
                order.ShippingMethod = shipMethodrefName;//shipMethodIdOrder.shipMethod?.refName;
			}
		}

		private static PaymentStatus GetPaymentStatusFromNetsuite(string id)
        {
            if (id == "2")
               return PaymentStatus.Pending;
            if (id == "1")
                return PaymentStatus.Paid;

            return PaymentStatus.Pending;
        }

        private void CreateOrderWebCustomer(Nop.Core.Domain.Orders.Order order, OrderNetsuite orderNetsuite)
        {
            var netsuiteId = 0;
            if (_workContext.CurrentCustomer.Companies.Any())
            {
                if (_workContext.WorkingCompany != null)
                {
                    netsuiteId = Convert.ToInt32(_workContext.WorkingCompany?.NetsuiteId);
                }
            }
            else
            {
                netsuiteId = Convert.ToInt32(_settingService.GetSetting("WebStoreCustomer.id").Value);
            }
            //validate Order In netsuite 
            var orderNetsuiteExist = GetOrderbyNetsuite(netsuiteId, order.Id);

            var NewOrder = true;

            if (orderNetsuiteExist != null)
            {
                foreach (var item in orderNetsuiteExist.Items)
                {
                    if (item.Document != order.tranId)
                        NewOrder = false;
                }
            }

            if (orderNetsuiteExist != null && NewOrder)
            {
                if (orderNetsuiteExist.Items.Count == 0)
                    CreateOrderWebCustomer(order, orderNetsuite, netsuiteId);
                else
                {
                    if (orderNetsuiteExist.Items.Count > 0)
                    {
                        foreach (var item in orderNetsuiteExist.Items)
						{
							order.tranId = item.Document;

							OrderStatusFromNetsuite(order, item.Status);

							_orderService.UpdateOrder(order);

							Location payStatus = new Location();
							if (order.PaymentStatus == PaymentStatus.Paid)
								payStatus.id = "1";
							else
								payStatus.id = "2";

							orderNetsuite.custbody_website_order_status = payStatus;
						}
					}
                }
            }
            else
                CreateOrderWebCustomer(order, orderNetsuite, netsuiteId);
        }

		private static void OrderStatusFromNetsuite(Nop.Core.Domain.Orders.Order order, string itemStatus)
		{
			if (itemStatus == "A")
				order.OrderStatus = OrderStatus.Pending;

			if (itemStatus == "B")
				order.OrderStatus = OrderStatus.Processing;

			if (itemStatus == "G")
				order.OrderStatus = OrderStatus.CompletedBilled;

			if (itemStatus == "F")
				order.OrderStatus = OrderStatus.Processing;

			if (itemStatus == "G")
				order.OrderStatus = OrderStatus.Complete;
		}

		private void CreateOrderWebCustomer(Nop.Core.Domain.Orders.Order order, OrderNetsuite orderNetsuite, int netsuiteId)
        {
            try
            {
                //get web customer in netsuite
                orderNetsuite.entity = _settingService.GetSetting("WebStoreCustomer.id").Value;
                orderNetsuite.custbody31 = order.Id.ToString();

                Location customForm = new Location();
                customForm.id = _settingService.GetSetting("WebStoreCustomer.customForm").Value; // "198";
                customForm.refName = "Web Order";
                orderNetsuite.customForm = customForm;

                //orderNetsuite.otherRefNum = order.Id;
                Location Location = new Location();
                Location.refName = _settingService.GetSetting("Netsuite.LocationRef").Value; // "Atlanta";
                orderNetsuite.Location = Location;

                if (order.ShippingAddress == null)
                {
                    order.ShippingAddress = order.BillingAddress;
                }

                Models.Order.BillingAddress billingAddress = new Models.Order.BillingAddress();
                billingAddress.first_name = order.BillingAddress?.FirstName;
                billingAddress.last_name = order.BillingAddress?.LastName;
                billingAddress.addr1 = order.BillingAddress?.Address1;
                billingAddress.addr2 = order.BillingAddress?.Address2;


                if (order.BillingAddress?.PhoneNumber != "0" && order.BillingAddress?.PhoneNumber?.Length > 7)
                    billingAddress.addrphone = order.BillingAddress?.PhoneNumber;

                billingAddress.city = order.BillingAddress?.City;

                Location countryBilling = new Location();
                countryBilling.id = order.BillingAddress?.Country?.TwoLetterIsoCode;
                countryBilling.refName = order.BillingAddress?.Country?.Name;
                billingAddress.country = countryBilling;

                billingAddress.state = order.BillingAddress?.StateProvince?.Abbreviation;
                billingAddress.zip = order.BillingAddress?.ZipPostalCode;

                orderNetsuite.billingAddress = billingAddress;


                Models.Order.ShippingAddress shippingAddress = new Models.Order.ShippingAddress();
                shippingAddress.first_name = order.ShippingAddress?.FirstName;
                shippingAddress.last_name = order.ShippingAddress?.LastName;
                shippingAddress.addr1 = order.ShippingAddress?.Address1;
                shippingAddress.addr2 = order.ShippingAddress?.Address2;

                shippingAddress.addressee = order.ShippingAddress?.FirstName + " " + order.ShippingAddress?.LastName;
                if (!string.IsNullOrEmpty(order.ShippingAddress.Company))
                {
                    shippingAddress.attention = order.ShippingAddress.Company;
                }

                if (order.ShippingAddress?.PhoneNumber != "0" && order.ShippingAddress?.PhoneNumber?.Length > 7)
                    shippingAddress.addrphone = order.ShippingAddress?.PhoneNumber;

                shippingAddress.city = order.ShippingAddress?.City;

                Location country = new Location();
                country.id = order.ShippingAddress?.Country?.TwoLetterIsoCode;
                country.refName = order.ShippingAddress?.Country?.Name;
                shippingAddress.country = country;

                shippingAddress.state = order.ShippingAddress?.StateProvince?.Abbreviation;
                shippingAddress.zip = order.ShippingAddress?.ZipPostalCode;

                orderNetsuite.shippingAddress = shippingAddress;
                orderNetsuite.custbody_webstore_email = order.BillingAddress?.Email;

                custbody8 custbody8 = new custbody8();
                custbody8.id = "5";

                orderNetsuite.custbody8 = custbody8;

                shipMethod shipMethod = new shipMethod();
                shipMethod.id = "1915";

                orderNetsuite.shippingCost = order.OrderShippingExclTax;

                if (order.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                {
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingUpsGroup.Name").Value)// "UPS Ground")
                        shipMethod.id = _settingService.GetSetting("ShippingUpsGroup.id").Value; // "1611";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAir.Name").Value)  //"UPS Next Day Air")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAir.id").Value;// "2521";

                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirEarlyAm.Name").Value)//"UPS Next Day Air Early AM")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value;//"2522";

                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirSaver.Name").Value)// "UPS Next Day Air Saver")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAirSaver.id").Value;//"2515";

                    if (order.ShippingMethod == _settingService.GetSetting("ShippingSecondDay.Name").Value) //"UPS Second Day Air")
                        shipMethod.id = _settingService.GetSetting("ShippingSecondDay.id").Value;// "2520";

                    if (order.ShippingMethod == _settingService.GetSetting("ShippingSecondDayAirAm.Name").Value) //"UPS Second Day Air AM")
                        shipMethod.id = _settingService.GetSetting("ShippingSecondDayAirAm.id").Value;//"2513";

                    if (order.ShippingMethod == _settingService.GetSetting("Shipping3DaySelect.Name").Value) //"UPS 3 Day Select")
                        shipMethod.id = _settingService.GetSetting("Shipping3DaySelect.id").Value;//"2514";

                    orderNetsuite.Location.refName = _settingService.GetSetting("ShippingPickupInStore.Nashville").Value;

                    orderNetsuite.Location.id = "";
                }
                if (order.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                {
                    shipMethod.id = _settingService.GetSetting("ShippingPickupInStore.id").Value;//"2697";
                    orderNetsuite.shippingCost = 0;

                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.Norcross").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Atlanta").Value;
                        orderNetsuite.Location = Location;
                    }
                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.OldHickory").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Nashville").Value;
                        orderNetsuite.Location = Location;
                    }
                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.Cincinnati").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Cincinnati").Value;
                        orderNetsuite.Location = Location;
                    }

                    orderNetsuite.Location.refName = Location.refName;
                    orderNetsuite.Location.id = "";
				}
				else
				{

                    if (!string.IsNullOrEmpty(order.WarehouseLocationNN))
                        orderNetsuite.Location.id = order.WarehouseLocationNN;

                }

                orderNetsuite.shipMethod = shipMethod;

                orderNetsuite.custbody58 = false;
                orderStatus orderStatus = new orderStatus();
                orderStatus.id = _settingService.GetSetting("NetsuiteOrderStatus.Pending").Value; //"A";
                orderNetsuite.orderStatus = orderStatus;

                Location paymentStatus = new Location();

                if (order.PaymentStatus == PaymentStatus.Paid)
                    paymentStatus.id = "1";
                else
                    paymentStatus.id = "2";

                orderNetsuite.custbody_website_order_status = paymentStatus;
                orderNetsuite.custbody_web_address = order.ShippingAddress?.Address1?.ToString() + ",  " + order.ShippingAddress?.Address2?.ToString() + ", " +
                                                      order.ShippingAddress?.City?.ToString() + " " + order.ShippingAddress?.StateProvince?.Abbreviation.ToString() + ", " +
                                                      order.ShippingAddress?.ZipPostalCode?.ToString() + ", " +
                                                      order.ShippingAddress?.Country?.Name.ToString() + " ";
                //items 
                Item Item = new Item();
                string discountCode = "";
                string discountCodeSend = "";
                string discountCodeTotalOrder = "";
                Item.items = new List<ItemDetail>();
                foreach (var x in order.OrderItems)
                {
                    Core.Domain.Catalog.Product product = x.Product;
                    decimal price = 0;
                    
                    GetPriceByQtyPricing(order, x, product, out price, out discountCode);

                    if (!string.IsNullOrEmpty(discountCode))
                        discountCodeSend = discountCode;

                    if (product != null)
                    {
                        ItemDetail ItemDetail = new ItemDetail
                        {
                            custcol_item_attribute = x.AttributeDescription,
                            amount = price * x.Quantity,
                            quantity = x.Quantity,
                            rate = price,
                            item = new InventoryItem
                            {
                                id = product.IdInventoryItem
                            }
                        };
                        Item.items.Add(ItemDetail);
                    }
                }
                
                orderNetsuite.item = Item;
                orderNetsuite.custbody_website_order_number = ModeAbb + order.Id.ToString();
                //orderNetsuite.custbody_tj_external_tax_amount = order.OrderTax;
                orderNetsuite.custbody_taxjar_external_amount_n_n = order.OrderTax;
               
                var IsCommercial = _genericAttributeService.GetAttribute<ShippingOption>(order.Customer,
                               NopCustomerDefaults.SelectedShippingOptionAttributeIsCommercial, _storeContext.CurrentStore.Id);
                if (IsCommercial != null)
                {
                    if (IsCommercial.IsCommercial)
                        orderNetsuite.shipIsResidential = false;
                    else
                        orderNetsuite.shipIsResidential = true;


                    
                }

                
                if (order.DiscountUsageHistory.Count()>0)
                {
                    foreach (var item in order.DiscountUsageHistory)
                    {
                        if(item.Discount?.CouponCode!=null && item.Discount?.CouponCode != "TestDiscount")
                            discountCodeTotalOrder = item.Discount.CouponCode;
                        //else
                        //    discountCodeTotalOrder = discountCodeTotalOrder + ", " + item.Discount.CouponCode;
                    }
                }
                if (order.Customer.IsGuest())
                    orderNetsuite.custbody_website_user_role = "Guest";
				else
                    orderNetsuite.custbody_website_user_role = "Registered";

                orderNetsuite.custbody_pickup_person_notes = order.PickupPersonNote;


                orderNetsuite.custbody_promotion = discountCodeTotalOrder;
                CreateOrder(orderNetsuite);

                //validate Order In netsuite 
                var OrderUpdate = GetOrderbyNetsuite(netsuiteId, order.Id);

                if (OrderUpdate != null)
                {
                    var orderInser = OrderUpdate.Items.Where(r => r.OrderNopId == ModeAbb + order.Id.ToString()).OrderByDescending(r => r.Document).FirstOrDefault();
                    if (orderInser != null)
                    {
                        order.tranId = orderInser.Document;
                        _orderService.UpdateOrder(order);
                    }

				}
				
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportOrderError:: CreateOrderWebCustomer:: order Id" + order.Id + "  orderNetsuiteId " + netsuiteId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

            }
        }

        private void GetPriceByQtyPricing(Nop.Core.Domain.Orders.Order order, OrderItem x, Core.Domain.Catalog.Product product,  out decimal price, out string discountCodeInfo)
        {
            
                //initial price
                price = x.UnitPriceExclTax;
                discountCodeInfo = "";
                var customerInfo = _workContext.CurrentCustomer;
                var companyInfo = _workContext.WorkingCompany;
                if (_workContext.CurrentCustomer.Id != order.CustomerId)
                {
                    customerInfo = _customerService.GetCustomerById(order.CustomerId);
                    companyInfo = customerInfo.CompanyCustomerMappings.Where(r => r.DefaultCompany == true).Select(r => r.Company).FirstOrDefault();
                }

                int id = Convert.ToInt32(x.ProductId);

                var applyDiscount = false;
                decimal DiscountPercentage = 0;
            //Discount Deal of the month
            foreach (var item in product.DiscountProductMappings)
            {
                if (order.CreatedOnUtc >= item.Discount.StartDateUtc && order.CreatedOnUtc <= item.Discount.EndDateUtc)
                {
                    if (item.Discount.DiscountType == Core.Domain.Discounts.DiscountType.AssignedToSkus)
                    {
                        if (item.Discount.DiscountPercentage == 0)
                        {
                            if (item.Discount.RequiresCouponCode)
                            {
                                DiscountPercentage += item.Discount.DiscountAmount;
                                discountCodeInfo = item.Discount.CouponCode;
                            }
                            applyDiscount = true;
                        }
                        else
                        {
                            if (item.Discount.RequiresCouponCode)
                            {
                                DiscountPercentage += item.Discount.DiscountPercentage;

                                discountCodeInfo = item.Discount.CouponCode;
                            }
                            applyDiscount = true;
                        }
                    }
                }
            }

                if (applyDiscount)
                {
                    //Validate ItemPricing
                    if (companyInfo?.ItemsPricing != null && companyInfo?.ItemsPricing.FirstOrDefault(s => s.ProductId == product.Id) != null)
                    {
                        price = companyInfo.ItemsPricing.FirstOrDefault(s => s.ProductId == product.Id).Price;
                    }
                    else
                    {
                        //Get Price By Information
                        //4= Role Guest

                        if (order.Customer.CustomerRoles.FirstOrDefault(s => s.Id == 4) != null || (order.Customer.NetsuitId == 0 && order.Customer.IsRegistered()))
                        {
                            //2= Price Level Retail & Web Store
                            if (product.PriceByQtyProduct.Where(s => s.PriceLevelId == 2 && x.Quantity >= s.Quantity).OrderByDescending(s => s.Quantity).FirstOrDefault() != null)
                                price = product.PriceByQtyProduct.Where(s => s.PriceLevelId == 2 && x.Quantity >= s.Quantity).OrderByDescending(s => s.Quantity).FirstOrDefault().Price;
                            else
                                price = product.Price;
                        }
                        else
                        {
                            if (companyInfo?.PriceLevelId != null && product.PriceByQtyProduct.Where(s => s.PriceLevelId == companyInfo?.PriceLevelId && x.Quantity >= s.Quantity).OrderByDescending(s => s.Quantity).FirstOrDefault() != null)
                                price = product.PriceByQtyProduct.Where(s => s.PriceLevelId == companyInfo?.PriceLevelId && x.Quantity >= s.Quantity).OrderByDescending(s => s.Quantity).FirstOrDefault().Price;
                            else
                                price = product.Price;
                        }

                    }
                }
            
        }

        private void CreateOrderAccountCustomer(Nop.Core.Domain.Orders.Order order, OrderNetsuite orderNetsuite)
        {
            var NetsuiteId = 0;
            if (order.CompanyId != null)
            {
                var companyNetsuite = _companyService.GetCompanyById(Convert.ToInt32(order.CompanyId));
                if(companyNetsuite!=null)
                    NetsuiteId = Convert.ToInt32(companyNetsuite.NetsuiteId);
            }

            var orderNetsuiteExist = new TransactionListDto();

            if (NetsuiteId != 0)
                orderNetsuiteExist = GetOrderbyNetsuite(Convert.ToInt32(NetsuiteId), order.Id);

            else
                orderNetsuiteExist = GetOrderbyNetsuite(Convert.ToInt32(order.CompanyId), order.Id);

            var NewOrder = true;

            foreach (var item in orderNetsuiteExist.Items)
            {
                if (item.Document != order.tranId)
                    NewOrder = false;
            }

            if (orderNetsuiteExist == null && NewOrder)
            {
                if (orderNetsuiteExist.Items.Count() > 0)
                {
                    foreach (var item in orderNetsuiteExist.Items)
                    {
                        order.tranId = item.Document;

                        OrderStatusFromNetsuite(order, item.Status);

                       
                        _orderService.UpdateOrder(order);

                        Location payStatus = new Location();
                        if (order.PaymentStatus == PaymentStatus.Paid)
                            payStatus.id = "1";
                        else
                            payStatus.id = "2";

                        orderNetsuite.custbody_website_order_status = payStatus;

                    }
                }
                else
                    SendNewOrderNetsuite(order, orderNetsuite);

            }
            else
                SendNewOrderNetsuite(order, orderNetsuite);

        }
        #endregion

        #region Update Orders
        private void UpdateOrderFromNetsuiteNopcommerce(TransactionDto item, Nop.Core.Domain.Orders.Order order, int CompanyId, TransactionDto Transaction)
        {
            try
            {
                
                if (order == null)
                {
                    order = new Nop.Core.Domain.Orders.Order();
                }
                var getInvoice = _invoice.GetInvoicesByTransId(CompanyId, Transaction.TranId);
                Invoice invoice = new Invoice();
                if (getInvoice == null)
                {

                    invoice.CreatedDate = Transaction.Createddate;
                    invoice.PONumber = Transaction.TranId;

                    if (order.Id != 0)
                        invoice.SaleOrderId = order.Id;

                    invoice.Total = Transaction.Foreigntotal;
                    invoice.InvoiceNo = Transaction.TranId;
                    invoice.Status = Transaction.Status;
                    invoice.LastModifiedDate = Transaction.Lastmodifieddate;
                    if (item != null)
                        invoice.InvoiceNetSuiteId = Convert.ToInt32(item.Orderid);

                    if (Transaction.duedate.ToString() == "1/1/0001 12:00:00 AM")
                        invoice.duedate = DateTime.Now;
                    else
                    {
                        invoice.duedate = Transaction.duedate;
                    }

                    invoice.foreigntotal = Transaction.Foreigntotal;
                    invoice.CompanyId = CompanyId;
                    invoice.foreignamountpaid = Transaction.foreignamountpaid;
                    invoice.foreignamountunpaid = Transaction.Foreignamountunpaid;
                    invoice.StatusName = Transaction.Status;
                    invoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);
                    invoice.StatusPaymentNP = false;

                    if (Transaction != null)
                    {
                        var InvoiceInfo = GetInvoiceFromNetsuite(Transaction.Orderid.ToString());
                        if (InvoiceInfo != null)
                        {
                            invoice.AmountDue = InvoiceInfo.amountRemaining;
                        }
                    }
                    _invoice.InsertInvoice(invoice);
                    
                }
                else
                {
                    getInvoice.CreatedDate = Transaction.Createddate;
                    getInvoice.PONumber = Transaction.TranId;

                    if (order != null)
                        getInvoice.SaleOrderId = order.Id;

                    getInvoice.Total = Transaction.Foreigntotal;
                    getInvoice.InvoiceNo = Transaction.TranId;
                    getInvoice.Status = Transaction.Status;
                    getInvoice.LastModifiedDate = Transaction.Lastmodifieddate;

                    if (item != null)
                        getInvoice.InvoiceNetSuiteId = Convert.ToInt32(item.Orderid);

                    if (Transaction.duedate.ToString() == "1/1/0001 12:00:00 AM")
                        getInvoice.duedate = DateTime.Now;
                    else
                    {
                        getInvoice.duedate = Transaction.duedate;
                    }
                    getInvoice.foreigntotal = Transaction.Foreigntotal;
                    getInvoice.CompanyId = CompanyId;
                    getInvoice.foreignamountpaid = Transaction.foreignamountpaid;
                    getInvoice.foreignamountunpaid = Transaction.Foreignamountunpaid;
                    getInvoice.StatusName = Transaction.Status;

                    if (Transaction != null)
                    {
                        var InvoiceInfo = GetInvoiceFromNetsuite(Transaction.Orderid.ToString());
                        if (InvoiceInfo != null)
                        {
                            if (InvoiceInfo.amountRemaining < getInvoice.AmountDue)
                            {
                                getInvoice.StatusPaymentNP = true;
                                getInvoice.AmountDue = InvoiceInfo.amountRemaining;
                            }
                           
                        }

                        if (InvoiceInfo != null)
                        {
                            if (InvoiceInfo.amountRemaining <= getInvoice.AmountDue)
                            {
                                getInvoice.StatusPaymentNP = true;
                                getInvoice.AmountDue = InvoiceInfo.amountRemaining;
                            }
                            
                        }

                        if (Transaction.Lastmodifieddate > getInvoice.LastModifiedDate)
                        {
                            getInvoice.Total = Transaction.Foreigntotal;
                            getInvoice.Status = Transaction.Status;
                            getInvoice.LastModifiedDate = Transaction.Lastmodifieddate;

                        }
                        else
                        {
                            if (Transaction.Status == "B")
                            {
                                getInvoice.StatusPaymentNP = true;
                                getInvoice.AmountDue = InvoiceInfo.amountRemaining;
                                getInvoice.PaymentStatusId= (int)PaymentStatus.Paid;
                            }
                        }
                        //if (InvoiceInfo.status.id == "Paid In Full")
                        //{
                        //    order.PaymentStatus = PaymentStatus.Paid;
                        //    _orderService.UpdateOrder(order);
                        //}
                    }

                    if (getInvoice.InvoiceNetSuiteId == 0)
                        getInvoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                    if (getInvoice!=null)
                        _invoice.UpdateOrder(getInvoice);

                   
                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                //_logger.Warning("ImportOrder Error: InsertInvoice Services NetsuitePlugin ", ex);
            }
        }

        private void UpdateOrderFromNetsuiteNopcommerce(int CompanyId, TransactionDto Transaction)
        {
            try
            {
                var getInvoice = _invoice.GetInvoicesByTransId(CompanyId, Transaction.TranId);
                Invoice invoice = new Invoice();
                if (getInvoice == null)
                {
                    if (Transaction.Status != "B")
                    {
                        invoice.CreatedDate = Transaction.Createddate;
                        invoice.PONumber = Transaction.TranId;
                        invoice.Total = Transaction.Foreigntotal;
                        invoice.InvoiceNo = Transaction.TranId;
                        invoice.Status = Transaction.Status;
                        invoice.LastModifiedDate = Transaction.Lastmodifieddate;

                        if (Transaction.duedate.ToString() == "1/1/0001 12:00:00 AM")
                            invoice.duedate = DateTime.Now;
                        else
                        {
                            invoice.duedate = Transaction.duedate;
                        }

                        invoice.foreigntotal = Transaction.Foreigntotal;
                        invoice.CompanyId = CompanyId;
                        invoice.foreignamountpaid = Transaction.foreignamountpaid;
                        invoice.foreignamountunpaid = Transaction.Foreignamountunpaid;
                        invoice.StatusName = Transaction.Status;
                        invoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);
                        invoice.StatusPaymentNP = false;

                        if (Transaction != null)
                        {
                            var InvoiceInfo = GetInvoiceFromNetsuite(Transaction.Orderid.ToString());
                            if (InvoiceInfo != null)
                            {
                                invoice.AmountDue = InvoiceInfo.amountRemaining;
                            }
                        }
                        _invoice.InsertInvoice(invoice);

					}
					else
					{

					}
                }
                else
                {
                    getInvoice.CreatedDate = Transaction.Createddate;
                    getInvoice.PONumber = Transaction.TranId;

                    getInvoice.Total = Transaction.Foreigntotal;
                    getInvoice.InvoiceNo = Transaction.TranId;
                    getInvoice.Status = Transaction.Status;
                    getInvoice.LastModifiedDate = Transaction.Lastmodifieddate;

                    if (Transaction.duedate.ToString() == "1/1/0001 12:00:00 AM")
                        getInvoice.duedate = DateTime.Now;
                    else
                    {
                        getInvoice.duedate = Transaction.duedate;
                    }
                    
                    getInvoice.foreigntotal = Transaction.Foreigntotal;
                    getInvoice.CompanyId = CompanyId;
                    getInvoice.foreignamountpaid = Transaction.foreignamountpaid;
                    getInvoice.foreignamountunpaid = Transaction.Foreignamountunpaid;
                    getInvoice.StatusName = Transaction.Status;

                    if (Transaction != null)
                    {
                        var InvoiceInfo = GetInvoiceFromNetsuite(Transaction.Orderid.ToString());
                        if (InvoiceInfo != null)
                        {
                            if (InvoiceInfo.amountRemaining < getInvoice.AmountDue)
                            {
                                getInvoice.StatusPaymentNP = true;
                                getInvoice.AmountDue = InvoiceInfo.amountRemaining;

                                if (InvoiceInfo.amountRemaining == 0)
                                {
                                    getInvoice.PaymentStatusId = (int)PaymentStatus.Paid;
                                }
                            }
                            else
                            {
                                if (InvoiceInfo.amountRemaining > 0 && !(InvoiceInfo.amountRemaining == getInvoice.AmountDue))
                                {
                                    getInvoice.StatusPaymentNP = true;
                                    getInvoice.AmountDue = InvoiceInfo.amountRemaining;

                                    getInvoice.PaymentStatusId = (int)PaymentStatus.PartialPay;

                                }
                                else
                                {
                                    if (InvoiceInfo.amountRemaining == 0)
                                    {
                                        getInvoice.StatusPaymentNP = true;
                                        getInvoice.AmountDue = InvoiceInfo.amountRemaining;
                                        getInvoice.PaymentStatusId = (int)PaymentStatus.Paid;
                                    }
                                    else
                                    {
                                        if (InvoiceInfo.amountRemaining == getInvoice.AmountDue)
                                        {
                                            getInvoice.StatusPaymentNP = true;
                                            getInvoice.AmountDue = InvoiceInfo.amountRemaining;
                                            getInvoice.PaymentStatusId = (int)PaymentStatus.Pending;

                                        }
                                    }

                                }
                            }

                        }

                        if (InvoiceInfo != null)
                        {
                            if (InvoiceInfo.amountRemaining <= getInvoice.AmountDue)
                            {
                                getInvoice.StatusPaymentNP = true;
                                getInvoice.AmountDue = InvoiceInfo.amountRemaining;

                                if (InvoiceInfo.amountRemaining == 0)
                                {
                                    getInvoice.PaymentStatusId = (int)PaymentStatus.Paid;
                                }
                            }

                        }

                        if (Transaction.Lastmodifieddate > getInvoice.LastModifiedDate)
                        {
                            getInvoice.Total = Transaction.Foreigntotal;
                            if (InvoiceInfo!=null)
                                getInvoice.AmountDue = InvoiceInfo.amountRemaining;

                            getInvoice.Status = Transaction.Status;
                            getInvoice.LastModifiedDate = Transaction.Lastmodifieddate;
                            getInvoice.StatusPaymentNP = true;
                        }
                        else
                        {
                            if (Transaction.Status == "B")
                            {
                                getInvoice.StatusPaymentNP = true;
                                if (InvoiceInfo == null)
                                {
                                    var getIdInvoice = GetTransactionInvoiceId(Transaction.Orderid.ToString());

                                    if (getIdInvoice != null)
                                    {
                                        if (getIdInvoice.Items.Count > 0)
                                        {
                                            foreach (var item in getIdInvoice.Items)
                                            {
                                                if (item.Foreignamountunpaid <= getInvoice.AmountDue)
                                                {
                                                    getInvoice.StatusPaymentNP = true;
                                                    getInvoice.AmountDue = item.Foreignamountunpaid;
                                                    getInvoice.PaymentStatusId = (int)PaymentStatus.Paid;
                                                }
                                            }
                                        }
                                    }
                                } 
                                //if(InvoiceInfo.amountRemaining!=null)
                                //    getInvoice.AmountDue = InvoiceInfo.amountRemaining;
                                //
                            }



                            else
                            {
                                if (InvoiceInfo == null)
                                {
                                    var getIdInvoice = GetTransactionInvoiceId(Transaction.Orderid.ToString());
                                   
                                    if (getIdInvoice != null)
                                    {
                                        if (getIdInvoice.Items.Count > 0)
                                        {
                                            foreach (var item in getIdInvoice.Items)
                                            {
                                                if (item.Foreignamountunpaid <= getInvoice.AmountDue)
                                                {
                                                    getInvoice.StatusPaymentNP = true;
                                                    getInvoice.AmountDue = item.Foreignamountunpaid;
                                                }
                                            }
                                        }
                                        

                                    }

                                }
                            }
                            
                        }
                    }

                    if (getInvoice.InvoiceNetSuiteId == 0)
                        getInvoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);


                    _invoice.UpdateOrder(getInvoice);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                //_logger.Warning("ImportOrder Error: InsertInvoice Services NetsuitePlugin ", ex);
            }
        }

        #endregion

        #region Get Orders and transactions

        private int GetOrderStatus(string orderStatus)
        {
            if(orderStatus==null)
                return (int)OrderStatus.Pending;

            if (orderStatus=="A")
                return (int)OrderStatus.Pending;

            if (orderStatus == "B")
                return (int)OrderStatus.Processing;

            if (orderStatus == "D")
                return (int)OrderStatus.BillingPartiallyFulfilled;

            if (orderStatus == "E")
                return (int)OrderStatus.PendingPartiallyFulfilled;

            if (orderStatus == "F")
                return (int)OrderStatus.PartiallyFulfilled;

            if (orderStatus == "G")
                return (int)OrderStatus.Complete;

            if (orderStatus == "H")
                return (int)OrderStatus.Closed;

            if (orderStatus.ToLowerInvariant().Contains("cancelled"))
                return (int)OrderStatus.Cancelled;

            return (int)OrderStatus.Pending;
        }
        private void GetOrderFromNetsuitePendingAndBilled(string NetsuiteId, int companyIdNp, string LastExecutionDate)
        {
            try
            {
                if (NetsuiteId == "6612")
                    companyIdNp = 6633;

                if (NetsuiteId == "6633")
                    companyIdNp = 6633;

                // Get  Pending approval
                var GetOrdersPendingBilling = _orderService.GetOrderFromNetsuitePendingFulfillment(Convert.ToInt32(companyIdNp));
                if (GetOrdersPendingBilling != null)
                {
                    if (GetOrdersPendingBilling.Count > 0)
                    {
                        var PendingBilling = GetOpenSalesbyNetsuite(Convert.ToInt32(NetsuiteId), LastExecutionDate);
                        if (PendingBilling != null)
                        {
                            foreach (var x in GetOrdersPendingBilling)
                            {
                                var OrderNetsuite = PendingBilling.Items?.Where(r => r.Document == x.tranId).FirstOrDefault();
                                if (OrderNetsuite != null)
                                {
                                    UpdateOrder(x, OrderNetsuite.Orderid);
                                    CreateUpdateInvoice(OrderNetsuite, Convert.ToInt32(OrderNetsuite.Orderid), x, Convert.ToInt32(NetsuiteId));
                                    GetTransactionOrderStatus(x, OrderNetsuite);
								}
								
                            }
                        }
                    }
                }


                // Get  Pending billing orders
                if (GetOrdersPendingBilling != null)
                {
                    if (GetOrdersPendingBilling.Count > 0)
                    {
                        var PendingBilling = GetPendingBillingSalesbyNetsuite(Convert.ToInt32(NetsuiteId), LastExecutionDate);
                        if (PendingBilling != null)
                        {
                            foreach (var item in GetOrdersPendingBilling)
                            {
                                var OrderNetsuite = PendingBilling.Items?.Where(r => r.Document == item.tranId).FirstOrDefault();
                                if (OrderNetsuite != null)
                                {
                                    UpdateOrder(item, OrderNetsuite.Orderid);
                                    CreateUpdateInvoice(OrderNetsuite, Convert.ToInt32(OrderNetsuite.Orderid), item, Convert.ToInt32(NetsuiteId));
                                    GetTransactionOrderStatus(item, OrderNetsuite);
                                }
                            }
                        }
                    }
                }

                // Get Pending Billing/Partially Fulfilled
                var GetOrdersPartiallyFulfilled = _orderService.GetOrderFromNetsuitePendingFulfillment(Convert.ToInt32(companyIdNp));
                if (GetOrdersPartiallyFulfilled != null)
                {
                    if (GetOrdersPartiallyFulfilled.Count > 0)
                    {
                        var PendingBilling = GetPendingBillingPartiallyFulfilledbyNetsuite(Convert.ToInt32(NetsuiteId), LastExecutionDate);
                        if (PendingBilling != null)
                        {
                            if (PendingBilling.Items.Count() > 0)
                            {
                                foreach (var item in GetOrdersPartiallyFulfilled)
                                {
                                    if (item.CustomOrderNumber == "SO111453")
                                    {

                                    }
                                    var OrderNetsuite = PendingBilling.Items?.Where(r => r.Document == item.tranId).FirstOrDefault();
                                    if (OrderNetsuite != null)
                                    {
                                        UpdateOrder(item, OrderNetsuite.Orderid);
                                        CreateUpdateInvoice(OrderNetsuite, Convert.ToInt32(OrderNetsuite.Orderid), item, Convert.ToInt32(NetsuiteId));

                                        var ItemFullFitmentStatus = GetTransactionOrderStatus(OrderNetsuite.Orderid.ToString());

                                        if (ItemFullFitmentStatus != null)
                                        {
                                            //foreach (var x in ItemFullFitmentStatus.items)
                                            //{
                                            if (ItemFullFitmentStatus.items.Count() > 0)
                                            {
                                                ItemFullfitmentDto getInfoStatusShipped = GetitemFulfillmentNetsuite(ItemFullFitmentStatus.items?.FirstOrDefault().transaction?.ToString());

                                                if (getInfoStatusShipped != null)
                                                {
                                                    if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Packed.ToString())
                                                    {
                                                        if (item.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                                                        {
                                                            item.ShippingStatus = ShippingStatus.ReadyforUPSPickup;
                                                        }
                                                        if (item.ShippingRateComputationMethodSystemName.Contains("Shipping.NNDelivery"))
                                                        {
                                                            item.ShippingStatus = ShippingStatus.ReadyforDelivery;
                                                        }
                                                        if (item.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                                                        {
                                                            item.ShippingStatus = ShippingStatus.ReadyforPickup;
                                                        }

                                                    }

                                                    if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Picked.ToString())
                                                    {
                                                        item.ShippingStatus = ShippingStatus.Picked;
                                                    }

                                                    if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Delivered.ToString())
                                                        item.ShippingStatus = ShippingStatus.Delivered;

                                                    if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Shipped.ToString())
                                                        item.ShippingStatus = ShippingStatus.Shipped;
                                                }
                                                //}
                                            }

                                            _orderService.UpdateOrder(item);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                // Get Billed
                var GetOrdersBilled = _orderService.GetOrderByCompanyByPendingBilling(Convert.ToInt32(companyIdNp));
                if (GetOrdersBilled != null)
                {
                    if (GetOrdersBilled.Count > 0)
                    {
                        var PendingBilling = GetBilledbyNetsuite(Convert.ToInt32(NetsuiteId), LastExecutionDate);
                        if (PendingBilling != null)
                        {
                            foreach (var item in GetOrdersBilled)
                            {
                                if (item.CustomOrderNumber == "SO111453")
                                {

                                }
                                var OrderNetsuite = PendingBilling.Items?.Where(r => r.Document == item.tranId).FirstOrDefault();
                                if (OrderNetsuite != null)
                                {
                                    UpdateOrder(item, OrderNetsuite.Orderid);
                                    CreateUpdateInvoice(OrderNetsuite, Convert.ToInt32(OrderNetsuite.Orderid), item, Convert.ToInt32(NetsuiteId));
                                    var ItemFullFitmentStatus = GetTransactionOrderStatus(OrderNetsuite.Orderid.ToString());

                                    if (ItemFullFitmentStatus != null)
                                    {
                                        //foreach (var x in ItemFullFitmentStatus.items)
                                        //{
                                        if (ItemFullFitmentStatus.items.Count() > 0)
                                        {
                                            ItemFullfitmentDto getInfoStatusShipped = GetitemFulfillmentNetsuite(ItemFullFitmentStatus.items?.FirstOrDefault().transaction?.ToString());

                                            if (getInfoStatusShipped != null)
                                            {
                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Packed.ToString())
                                                {
                                                    if (item.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                                                    {
                                                        item.ShippingStatus = ShippingStatus.ReadyforUPSPickup;
                                                    }
                                                    if (item.ShippingRateComputationMethodSystemName.Contains("Shipping.NNDelivery"))
                                                    {
                                                        item.ShippingStatus = ShippingStatus.ReadyforDelivery;
                                                    }
                                                    if (item.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                                                    {
                                                        item.ShippingStatus = ShippingStatus.ReadyforPickup;
                                                    }

                                                }

                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Picked.ToString())
                                                {
                                                    item.ShippingStatus = ShippingStatus.Picked;
                                                }

                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Delivered.ToString())
                                                    item.ShippingStatus = ShippingStatus.Delivered;

                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Shipped.ToString())
                                                    item.ShippingStatus = ShippingStatus.Shipped;
                                            }
                                            //}
                                        }

                                        _orderService.UpdateOrder(item);
                                    }
                                }
                            }
                        }
                    }
                }

                // Partially Fulfilled
                var GetOrderPartiallyFulfilled = _orderService.GetOrderByCompanyByPendingBilling(Convert.ToInt32(companyIdNp));
                if (GetOrderPartiallyFulfilled != null)
                {
                    if (GetOrderPartiallyFulfilled.Count > 0)
                    {
                        var PendingBilling = GetPartiallyFulfilledbyNetsuite(Convert.ToInt32(NetsuiteId), LastExecutionDate);
                        if (PendingBilling != null)
                        {
                            foreach (var item in GetOrderPartiallyFulfilled)
                            {
                                if (item.CustomOrderNumber == "SO111453")
                                {

                                }
                                var OrderNetsuite = PendingBilling.Items?.Where(r => r.Document == item.tranId).FirstOrDefault();
                                if (OrderNetsuite != null)
                                {
                                    UpdateOrder(item, OrderNetsuite.Orderid);
                                    CreateUpdateInvoice(OrderNetsuite, Convert.ToInt32(OrderNetsuite.Orderid), item, Convert.ToInt32(NetsuiteId));
                                    var ItemFullFitmentStatus = GetTransactionOrderStatus(OrderNetsuite.Orderid.ToString());

                                    if (ItemFullFitmentStatus != null)
                                    {
                                        //foreach (var x in ItemFullFitmentStatus.items)
                                        //{
                                        if (ItemFullFitmentStatus.items.Count() > 0)
                                        {
                                            ItemFullfitmentDto getInfoStatusShipped = GetitemFulfillmentNetsuite(ItemFullFitmentStatus.items?.FirstOrDefault().transaction?.ToString());

                                            if (getInfoStatusShipped != null)
                                            {
                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Packed.ToString())
                                                {
                                                    if (item.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                                                    {
                                                        item.ShippingStatus = ShippingStatus.ReadyforUPSPickup;
                                                    }
                                                    if (item.ShippingRateComputationMethodSystemName.Contains("Shipping.NNDelivery"))
                                                    {
                                                        item.ShippingStatus = ShippingStatus.ReadyforDelivery;
                                                    }
                                                    if (item.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                                                    {
                                                        item.ShippingStatus = ShippingStatus.ReadyforPickup;
                                                    }

                                                }

                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Picked.ToString())
                                                {
                                                    item.ShippingStatus = ShippingStatus.Picked;
                                                }

                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Delivered.ToString())
                                                    item.ShippingStatus = ShippingStatus.Delivered;

                                                if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Shipped.ToString())
                                                    item.ShippingStatus = ShippingStatus.Shipped;
                                            }
                                            //}
                                        }

                                        _orderService.UpdateOrder(item);
                                    }
                                }
                            }
                        }
                    }
                }

                // Cancel or closed orders  GetCancelledClosedOrdersbyNetsuite
                var GetOrderCancelOrClosed = _orderService.GetOrderByCompanyByPendingBilling(Convert.ToInt32(companyIdNp));
                if (GetOrderCancelOrClosed != null)
                {
                    if (GetOrderCancelOrClosed.Count > 0)
                    {
                        var closedOrders = GetCancelledClosedOrdersbyNetsuite(Convert.ToInt32(NetsuiteId), LastExecutionDate);
                        if (closedOrders != null)
                        {
                            foreach (var item in GetOrderCancelOrClosed)
                            {
                                
                                var OrderNetsuite = closedOrders.Items?.Where(r => r.Document == item.tranId).FirstOrDefault();
                                
                                if (OrderNetsuite != null)
                                {
                                    if(!string.IsNullOrEmpty(OrderNetsuite.Orderid))
									{
                                     
                                            item.OrderStatusId = (int)OrderStatus.Closed;
                                            item.Deleted = true;
                                            _orderService.UpdateOrder(item);
                                        
                                    }

                                }
                            }
                        }
                    }
                }

                
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportOrderError:: CreateOrderNopCommerceByNetsuite:: companyIdNp" + companyIdNp + "  orderNetsuiteId " + NetsuiteId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private void GetTransactionOrderStatus(Nop.Core.Domain.Orders.Order item, TransactionDto OrderNetsuite)
        {
            var ItemFullFitmentStatus = GetTransactionOrderStatus(OrderNetsuite.Orderid.ToString());

            if (ItemFullFitmentStatus != null)
            {
                //foreach (var x in ItemFullFitmentStatus.items)
                //{
                if (ItemFullFitmentStatus.items.Count() > 0)
                {
                    ItemFullfitmentDto getInfoStatusShipped = GetitemFulfillmentNetsuite(ItemFullFitmentStatus.items?.FirstOrDefault().transaction?.ToString());

                    if (getInfoStatusShipped != null)
                    {
                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Packed.ToString())
                        {
                            if (item.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                            {
                                item.ShippingStatus = ShippingStatus.ReadyforUPSPickup;
                            }
                            if (item.ShippingRateComputationMethodSystemName.Contains("Shipping.NNDelivery"))
                            {
                                item.ShippingStatus = ShippingStatus.ReadyforDelivery;
                            }
                            if (item.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                            {
                                item.ShippingStatus = ShippingStatus.ReadyforPickup;
                            }

                        }

                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Picked.ToString())
                        {
                            item.ShippingStatus = ShippingStatus.Picked;
                        }

                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Delivered.ToString())
                            item.ShippingStatus = ShippingStatus.Delivered;

                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Shipped.ToString())
                            item.ShippingStatus = ShippingStatus.Shipped;
                    }
                    //}
                }

                _orderService.UpdateOrder(item);
            }
        }

        private void UpdateOrder(Nop.Core.Domain.Orders.Order item, string Orderid)
		{
            if(Orderid!=null)
			{
                var orderNetsuite = GetOrderId(Convert.ToInt32(Orderid));

                if (item.OrderTotal !=(decimal) orderNetsuite.total)
                    item.OrderTotal = (decimal)orderNetsuite.total;

                if (item.OrderShippingInclTax != (decimal)orderNetsuite.shippingCost)
				{
                    item.OrderShippingInclTax = (decimal)orderNetsuite.shippingCost;
                    item.OrderShippingExclTax = (decimal)orderNetsuite.shippingCost;
                }

                if (item.OrderSubtotalInclTax != (decimal)orderNetsuite.subtotal)
				{
                    item.OrderSubtotalExclTax = (decimal)orderNetsuite.subtotal;
                    item.OrderSubtotalInclTax = (decimal)orderNetsuite.subtotal;
                }


                if (string.IsNullOrEmpty(orderNetsuite.custbody_taxjar_external_amount_n_n))
                {
                    item.OrderTax = orderNetsuite.custbody_tax_mirror;
                    item.OrderSubtotalInclTax = orderNetsuite.custbody_tax_mirror;
                    item.OrderSubtotalExclTax = orderNetsuite.custbody_tax_mirror;
                }
                var items = GetOrderItems(Convert.ToInt32(Orderid));

                if (items.Count() > 0)
                {
                    UpdateOrderItemsFromNetsuite(items, item);
                }
                _orderService.UpdateOrder(item);

            }


		}
		private void GetOrderFromNetsuitePendingFulfillment(string NetsuiteId, string LastExecutionDate)
        {
            char delimitador = '/';
            string dateLimit = "";
            int TotalResults = 1;
            // Get Order Pending Fulfillment
            var GetOrdersPendingFulfillment = _orderService.GetOrderFromNetsuitePendingFulfillment(Convert.ToInt32(NetsuiteId));
            var AllPendingOrdersWebLastModified = GetPendingSalesbyNetsuiteWebAccount(Convert.ToInt32(NetsuiteId), null, LastExecutionDate,null);

            
                foreach (var item in GetOrdersPendingFulfillment)
                {
                if (AllPendingOrdersWebLastModified != null)
                {
                    if (AllPendingOrdersWebLastModified.Items.Count() > 0)
                    {
                        var OrderNetsuite = AllPendingOrdersWebLastModified.Items.Where(r => r.OrderNopId == ModeAbb + item.Id.ToString() && r.Document == item.tranId).FirstOrDefault();
                        //GetPendingSalesbyNetsuiteWebAccount(Convert.ToInt32(NetsuiteId), item.Id, LastExecutionDate);

                        //var OrderNetsuite = PendingBilling.Items?.Where(r => r.Document == item.tranId).FirstOrDefault();
                        if (OrderNetsuite != null)
                        {
                            UpdateOrder(item, OrderNetsuite.Orderid);
                            CreateUpdateInvoice(OrderNetsuite, Convert.ToInt32(OrderNetsuite.Orderid), item, Convert.ToInt32(NetsuiteId));
                            var ItemFullFitmentStatus = GetTransactionOrderStatus(OrderNetsuite.Orderid.ToString());

                            if (ItemFullFitmentStatus != null)
                            {
                                //foreach (var x in ItemFullFitmentStatus.items)
                                //{
                                if (ItemFullFitmentStatus.items.Count() > 0)
                                {
                                    ItemFullfitmentDto getInfoStatusShipped = GetitemFulfillmentNetsuite(ItemFullFitmentStatus.items?.FirstOrDefault().transaction?.ToString());

                                    if (getInfoStatusShipped != null)
                                    {
                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Packed.ToString())
                                        {
                                            if (item.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                                            {
                                                item.ShippingStatus = ShippingStatus.ReadyforUPSPickup;
                                            }
                                            if (item.ShippingRateComputationMethodSystemName.Contains("Shipping.NNDelivery"))
                                            {
                                                item.ShippingStatus = ShippingStatus.ReadyforDelivery;
                                            }
                                            if (item.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                                            {
                                                item.ShippingStatus = ShippingStatus.ReadyforPickup;
                                            }

                                        }

                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Picked.ToString())
                                        {
                                            item.ShippingStatus = ShippingStatus.Picked;
                                        }

                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Delivered.ToString())
                                            item.ShippingStatus = ShippingStatus.Delivered;

                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Shipped.ToString())
                                            item.ShippingStatus = ShippingStatus.Shipped;
                                    }
                                    //}
                                }

                                _orderService.UpdateOrder(item);
                            }
                        }

                    }
                }
            }
            


            var AllGetOpenSalesbyNetsuite = GetOpenSalesbyNetsuite(Convert.ToInt32(NetsuiteId), LastExecutionDate);

            foreach (var item in GetOrdersPendingFulfillment)
            {
                if (AllGetOpenSalesbyNetsuite != null)
                {
                    if (AllGetOpenSalesbyNetsuite.Items.Count() > 0)
                    {
                        var OrderNetsuite = AllGetOpenSalesbyNetsuite.Items.Where(r => r.OrderNopId == ModeAbb + item.Id.ToString() && r.Document == item.tranId).FirstOrDefault();
                        //GetPendingSalesbyNetsuiteWebAccount(Convert.ToInt32(NetsuiteId), item.Id, LastExecutionDate);

                        //var OrderNetsuite = PendingBilling.Items?.Where(r => r.Document == item.tranId).FirstOrDefault();
                        if (OrderNetsuite != null)
                        {
                            UpdateOrder(item, OrderNetsuite.Orderid);
                            CreateUpdateInvoice(OrderNetsuite, Convert.ToInt32(OrderNetsuite.Orderid), item, Convert.ToInt32(NetsuiteId));
                            var ItemFullFitmentStatus = GetTransactionOrderStatus(OrderNetsuite.Orderid.ToString());

                            if (ItemFullFitmentStatus != null)
                            {
                                //foreach (var x in ItemFullFitmentStatus.items)
                                //{
                                if (ItemFullFitmentStatus.items.Count() > 0)
                                {
                                    ItemFullfitmentDto getInfoStatusShipped = GetitemFulfillmentNetsuite(ItemFullFitmentStatus.items?.FirstOrDefault().transaction?.ToString());

                                    if (getInfoStatusShipped != null)
                                    {
                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Packed.ToString())
                                        {
                                            if (item.ShippingRateComputationMethodSystemName== "Shipping.UPS")
                                            {
                                                item.ShippingStatus = ShippingStatus.ReadyforUPSPickup;
                                            }
                                            if (item.ShippingRateComputationMethodSystemName.Contains("Shipping.NNDelivery"))
                                            {
                                                item.ShippingStatus = ShippingStatus.ReadyforDelivery;
                                            }
                                            if (item.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                                            {
                                                item.ShippingStatus = ShippingStatus.ReadyforPickup;
                                            }

                                        }

                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Picked.ToString())
                                        {
                                            item.ShippingStatus = ShippingStatus.Picked;
                                        }

                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Delivered.ToString())
                                            item.ShippingStatus = ShippingStatus.Delivered;

                                        if (getInfoStatusShipped.shipStatus.refName == ShippingStatus.Shipped.ToString())
                                            item.ShippingStatus = ShippingStatus.Shipped;
                                    }
                                    //}
                                }

                                _orderService.UpdateOrder(item);
                            }
                        }

                    }
                }
            }

            if (AllPendingOrdersWebLastModified.Links.Count > 1)
            {
                for (int i = 0; i < TotalResults; i += 1000)
                {
                    dateLimit = "?limit=1000&offset=" + i;
                    //Get Customer List from Netsuite 
                    var ListOrder = GetPendingSalesbyNetsuiteWebAccount(Convert.ToInt32(NetsuiteId), null, LastExecutionDate,dateLimit);
                    if (ListOrder != null)
                    {
                        TotalResults = ListOrder.TotalResults;

                        foreach (var itemNew in ListOrder.Items)
                        {
                            var OrderNetsuite = GetOrdersPendingFulfillment.Where(r => r.Id.ToString() == ModeAbb + itemNew.OrderNopId && r.tranId == itemNew.Document).FirstOrDefault();

                            if (OrderNetsuite == null)
                            {
                                var customer = _customerService.GetChildsByNetsuitId(Convert.ToInt32(itemNew.Customerid)).FirstOrDefault();
                                CreateOrderbyNetSuite(Convert.ToInt32(itemNew.Orderid), customer, itemNew);


                            }
                        }

                    }
                }
            }
            
       }
        public void GetTransactionInfo(Customer customer, string LastExecutionDate = null)
        {
            //1. Get  sales order by customer login
            var transactionList = GetOpenSalesbyNetsuiteLastUpdate(LastExecutionDate);
            _logger.Warning("GetOpenSalesbyNetsuite" + transactionList);

            if (transactionList != null)
            {
                _logger.Warning("after GetOpenSalesbyNetsuite" + transactionList.Items.Count);

                foreach (var item in transactionList.Items)
                {
                    _logger.Warning("after item" + item.Document);

                    if (!string.IsNullOrEmpty(item.Document))
                    {
                        _logger.Warning("after item IsNullOrEmpty Document" + item.Document);

                        int orderId = Convert.ToInt32(item.Orderid);
                        var order = _orderService.GetOrderByNetsuiteId(item.Document);

                        _logger.Warning("after item IsNullOrEmpty Document" + orderId + "--" + order);


                        if (order == null)
                        {
                            CreateOrderbyNetSuite(orderId, customer);
                            order = _orderService.GetOrderByNetsuiteId(item.Document);
                        }



                        var invoiceNop = _invoice.GetInvoicesByCustomerOrderId(customer.Id, order.Id);
                        var Transaction = GetTransactionInvoice(item.Orderid, customer.Parent).Items?.FirstOrDefault();

                        if (invoiceNop == null)
                        {

                            if (Transaction != null)
                            {
                                _logger.Warning("get after Transaction!=null " + Transaction.TranId);

                                //var InvoiceNetsuite = GetInvoice(Convert.ToInt32(Transaction.Orderid));

                                Invoice invoice = new Invoice();
                                //invoice.Id = Transaction.Orderid;
                                invoice.CreatedDate = Transaction.Createddate;
                                invoice.PONumber = Transaction.TranId;
                                invoice.SaleOrderId = order.Id;
                                invoice.CustomerId = customer.Id;
                                //invoice.Subtotal = Transaction.Foreigntotal;
                                invoice.Total = Transaction.Foreigntotal;
                                invoice.InvoiceNo = Transaction.TranId;
                                invoice.Status = Transaction.Status;
                                invoice.LastModifiedDate = Transaction.Lastmodifieddate;
                                invoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                try
                                {
                                    _invoice.InsertInvoice(invoice);

                                    if (Transaction.Status == "A")
                                        order.PaymentStatus = PaymentStatus.Pending;
                                    if (Transaction.Status == "B")
                                        order.PaymentStatus = PaymentStatus.Paid;

                                    _logger.Warning("get before UpdateOrder " + Transaction.Status);

                                    _orderService.UpdateOrder(order);
                                }
                                catch (Exception ex)
                                {
                                    _notificationService.ErrorNotification(ex.Message);
                                    _logger.Warning("ImportOrderError:: InsertInvoice:: InvoiceNetSuiteId" + invoice.InvoiceNetSuiteId + "  company " + invoice.CompanyId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

                                }

                            }
                        }
                        else
                        {
                            if (Transaction != null)
                            {
                                if (Transaction.Lastmodifieddate > invoiceNop.LastModifiedDate)
                                {
                                    //invoice.Subtotal = Transaction.Foreigntotal;
                                    invoiceNop.Total = Transaction.Foreigntotal;
                                    invoiceNop.Status = Transaction.Status;
                                    invoiceNop.LastModifiedDate = Transaction.Lastmodifieddate;


                                    if (invoiceNop.InvoiceNetSuiteId == 0)
                                        invoiceNop.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                    _invoice.UpdateOrder(invoiceNop);


                                }
                            }
                        }

                        if (item.Status != "G")
                        {
                            var Order = GetOrderId(orderId);
                            if (Order.shipMethod?.id == _settingService.GetSetting("ShippingUpsGroup.id").Value ||
                                  Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAir.id").Value
                               || Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value ||
                                  Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value ||
                                  Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDay.id").Value ||
                                  Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value ||
                                  Order.shipMethod?.id == _settingService.GetSetting("Shipping3DaySelect.id").Value)
                            {
                                if (Order.linkedTrackingNumbers != null)
                                {
                                    string[] tracking = Order.linkedTrackingNumbers.Split(' ');
                                    foreach (var t in tracking)
                                    {
                                        var trackingNumber = t;
                                        var ExistTracking = order.Shipments.Where(r => r.TrackingNumber == trackingNumber).FirstOrDefault();

                                        if (ExistTracking == null)
                                        {
                                            Shipment shipment = new Shipment
                                            {
                                                OrderId = order.Id,
                                                TrackingNumber = trackingNumber,
                                                TotalWeight = null,
                                                ShippedDateUtc = null,
                                                DeliveryDateUtc = null,
                                                AdminComment = "",
                                                CreatedOnUtc = DateTime.UtcNow
                                            };
                                            if (Order.shipDate != null)
                                                shipment.ShippedDateUtc = Convert.ToDateTime(Order.shipDate);


                                            foreach (var items in order.OrderItems)
                                            {
                                                ShipmentItem it = new ShipmentItem();

                                                it.OrderItemId = items.Id;
                                                it.Quantity = items.Quantity;

                                                shipment.ShipmentItems.Add(it);
                                            }
                                            order.Shipments.Add(shipment);
                                        }
                                    }
                                    order.ShippingMethod = Order.shipMethod?.refName;
                                    order.OrderShippingInclTax = Convert.ToDecimal(Order.shippingCost);

                                }

                                order.ShippingMethod = Order.shipMethod?.refName;
                                order.ShippingRateComputationMethodSystemName = "Shipping.UPS";
                            }
                            //Pickup.PickupInStore
                            if (Order.shipMethod?.id == _settingService.GetSetting("ShippingPickupInStore.id").Value)//"2691")
                            {
                                //validar la direccion del address
                                //order.PickupAddress= Order.
                                order.ShippingMethod = "Pickup at Atlanta Office";
                                order.ShippingRateComputationMethodSystemName = "Pickup.PickupInStore";
                            }
                            //Shipping.NNDelivery
                            if (Order.shipMethod?.id == "2706" || Order.shipMethod?.id == _settingService.GetSetting("ShippingNNDelivery.id").Value)// "2698")
                            {
                                order.ShippingMethod = "N&N Delivery";
                                order.ShippingRateComputationMethodSystemName = "Shipping.NNDelivery";
                                //validar la direccion del shipping
                                //order.PickupAddress= Order.
                            }
                            //Freight
                            if (Order.shipMethod?.id == "1915")
                            {
                                order.ShippingMethod = Order.shipMethod?.refName;
                            }

                            if (Order.custbody10 != null)
                            {
                                order.ShippingMethod = Order.custbody10?.refName;

                            }

                           // if (Order.status?.id != "E")
                                order.OrderStatusId = GetOrderStatus(Order.status?.id);


                            if (Order.orderStatus?.id == "G")
                                order.PaymentStatus = PaymentStatus.Paid;
                            else
                                order.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status?.id.ToString());

                            //order.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status.id);

                            _orderService.UpdateOrder(order);
                        }



                    }
                }
            }
            //2. Get Status Orders

            //validate tracking number


            //get saler order status by 
        }
        public void GetTransactionInfo(string LastExecutionDate = null)
        {
            //1. Get  sales order by customer login
            var transactionList = GetOpenSalesbyNetsuiteLastUpdate(LastExecutionDate);
            _logger.Warning("GetOpenSalesbyNetsuite" + transactionList);

            if (transactionList != null)
            {
                _logger.Warning("after GetOpenSalesbyNetsuite" + transactionList.Items.Count);

                foreach (var item in transactionList.Items)
                {
                    _logger.Warning("after item" + item.Document);

                    if (!string.IsNullOrEmpty(item.Document))
                    {
                        _logger.Warning("after item IsNullOrEmpty Document" + item.Document);

                        int orderId = Convert.ToInt32(item.Orderid);
                        var order = _orderService.GetOrderByNetsuiteId(item.Document);

                        _logger.Warning("after item IsNullOrEmpty Document" + orderId + "--" + order);

                        var customer = _customerService.GetCustomerByNetsuitId(Convert.ToInt32(item.Customerid));
                        if (order == null)
                        {
                            CreateOrderbyNetSuite(orderId, customer);
                            order = _orderService.GetOrderByNetsuiteId(item.Document);
                        }



                        var invoiceNop = _invoice.GetInvoicesByCustomerOrderId(customer.Id, order.Id);
                        var Transaction = GetTransactionInvoice(item.Orderid, customer.Parent).Items?.FirstOrDefault();

                        if (invoiceNop == null)
                        {

                            if (Transaction != null)
                            {
                                _logger.Warning("get after Transaction!=null " + Transaction.TranId);

                                //var InvoiceNetsuite = GetInvoice(Convert.ToInt32(Transaction.Orderid));

                                Invoice invoice = new Invoice();
                                //invoice.Id = Transaction.Orderid;
                                invoice.CreatedDate = Transaction.Createddate;
                                invoice.PONumber = Transaction.TranId;
                                invoice.SaleOrderId = order.Id;
                                invoice.CustomerId = customer.Id;
                                //invoice.Subtotal = Transaction.Foreigntotal;
                                invoice.Total = Transaction.Foreigntotal;
                                invoice.InvoiceNo = Transaction.TranId;
                                invoice.Status = Transaction.Status;
                                invoice.LastModifiedDate = Transaction.Lastmodifieddate;
                                invoice.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                try
                                {
                                     _invoice.InsertInvoice(invoice);
                                   
                                      order.PaymentStatus = GetPaymentStatusFromNetsuite(Transaction.Status);

                                    _orderService.UpdateOrder(order);
                                }
                                catch (Exception ex)
                                {
                                    _notificationService.ErrorNotification(ex.Message);
                                    _logger.Warning("ImportOrderError:: InsertInvoice:: InvoiceNetSuiteId" + invoice.InvoiceNetSuiteId + "  company " + invoice.CompanyId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                                }

                            }
                        }
                        else
                        {
                            if (Transaction != null)
                            {
                                if (Transaction.Lastmodifieddate > invoiceNop.LastModifiedDate)
                                {
                                    //invoice.Subtotal = Transaction.Foreigntotal;
                                    invoiceNop.Total = Transaction.Foreigntotal;
                                    invoiceNop.Status = Transaction.Status;
                                    invoiceNop.LastModifiedDate = Transaction.Lastmodifieddate;

                                    if (invoiceNop.InvoiceNetSuiteId == 0)
                                        invoiceNop.InvoiceNetSuiteId = Convert.ToInt32(Transaction.Orderid);

                                    _invoice.UpdateOrder(invoiceNop);
                                }
                            }
                        }

                        if (item.Status != "G")
                        {
                    var Order = GetOrderId(orderId);
                    if (Order.shipMethod?.id == _settingService.GetSetting("ShippingUpsGroup.id").Value ||
                      Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAir.id").Value
                   || Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value ||
                      Order.shipMethod?.id == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value ||
                      Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDay.id").Value ||
                      Order.shipMethod?.id == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value ||
                      Order.shipMethod?.id == _settingService.GetSetting("Shipping3DaySelect.id").Value)
                            {
                                if (Order.linkedTrackingNumbers != null)
                                {
                                    string[] tracking = Order.linkedTrackingNumbers.Split(' ');
                                    foreach (var t in tracking)
                                    {
                                        var trackingNumber = t;
                                        var ExistTracking = order.Shipments.Where(r => r.TrackingNumber == trackingNumber).FirstOrDefault();

                                        if (ExistTracking == null)
                                        {
                                            Shipment shipment = new Shipment
                                            {
                                                OrderId = order.Id,
                                                TrackingNumber = trackingNumber,
                                                TotalWeight = null,
                                                ShippedDateUtc = null,
                                                DeliveryDateUtc = null,
                                                AdminComment = "",
                                                CreatedOnUtc = DateTime.UtcNow
                                            };
                                            if (Order.shipDate != null)
                                                shipment.ShippedDateUtc = Convert.ToDateTime(Order.shipDate);

                                            foreach (var items in order.OrderItems)
                                            {
                                                ShipmentItem it = new ShipmentItem();

                                                it.OrderItemId = items.Id;
                                                it.Quantity = items.Quantity;

                                                shipment.ShipmentItems.Add(it);
                                            }

                                            order.Shipments.Add(shipment);
                                        }
                                    }
                                    order.ShippingMethod = Order.shipMethod?.refName;
                                    order.OrderShippingInclTax = Convert.ToDecimal(Order.shippingCost);

                                }
                            }

                            if (Order.custbody10 != null)
                            {
                                order.ShippingMethod = Order.custbody10?.refName;

                            }
                            //if (Order.status?.id != "E")
                                order.OrderStatusId = GetOrderStatus(Order.status?.id);

                            if (Order.orderStatus?.id == "G")
                                order.PaymentStatus = PaymentStatus.Paid;
                            else
                                order.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status?.id.ToString());

                          //  order.PaymentStatus = GetPaymentStatusFromNetsuite(Order.custbody_website_order_status.id);

                            _orderService.UpdateOrder(order);
                        }



                    }
                }
            }
            //2. Get Status Orders

            //validate tracking number


            //get saler order status by 
        }
        #endregion

        #region SendOrderNetsuite
        public void SendOrderNetsuite(Nop.Core.Domain.Orders.Order order)
        {
            TestMode = Convert.ToBoolean(_settingService.GetSetting("NetSuiteConnector.TestMode").Value);

            if (TestMode)
                ModeAbb = "NCTest_";

            try
            {
                OrderNetsuite orderNetsuite = new OrderNetsuite();
                var NetsuiteId = 0;
                if (order.CompanyId != null)
                {
                    var companyNetsuite = _companyService.GetCompanyById(Convert.ToInt32(order.CompanyId));
                    if(companyNetsuite!=null)
                        NetsuiteId = Convert.ToInt32(companyNetsuite.NetsuiteId);
                }

                var orderNetsuiteExist = new TransactionListDto();

                if (NetsuiteId != 0)
                    orderNetsuiteExist = GetOrderbyNetsuite(Convert.ToInt32(NetsuiteId), order.Id);

                else
                    orderNetsuiteExist = GetOrderbyNetsuite(Convert.ToInt32(order.CompanyId), order.Id);

                var NewOrder = true;
                bool update = false;

				if (orderNetsuiteExist!= null)
				{
                    foreach (var item in orderNetsuiteExist?.Items)
                    {
                        if (item.Document != order.tranId)
                            NewOrder = false;
                    }

                    if (orderNetsuiteExist.Items.Count() > 0 && !NewOrder)
                    {
                        foreach (var item in orderNetsuiteExist.Items)
                            update = SendUpdateOrderNetsuite(order, item);

                        if (update)
                        {
                            //validate Order Sync ok 
                            var orderValid = _orderService.GetOrderById(order.Id);
                            bool validOrderSyncNetsuite = false;

                            if (!string.IsNullOrEmpty(orderValid.tranId))
                                validOrderSyncNetsuite = true;

                            _pendingOrdersToSyncService.InsertOrUpdatePendingOrder(new PendingOrdersToSync
                            {
                                OrderId = order.Id,
                                SuccessDate = DateTime.UtcNow,
                                Synchronized = validOrderSyncNetsuite,
                            });
                            order.statusNetsuite = 2;
                        }
                    }
                    else
                    {
                        if (order.Customer.Parent != 0)
                            CreateOrderAccountCustomer(order, orderNetsuite);
                        else
                        {
                            //create order if it is paid and if it is web guest or web account customer
                            CreateOrderWebCustomer(order, orderNetsuite);
                        }

                        //validate Order Sync ok 
                        var orderValid = _orderService.GetOrderById(order.Id);
                        bool validOrderSyncNetsuite = false;

                        if (!string.IsNullOrEmpty(orderValid.tranId))
						{
                            validOrderSyncNetsuite = true;
                            order.statusNetsuite = 2;
                        }else
                            order.statusNetsuite = 0;


                        _pendingOrdersToSyncService.InsertOrUpdatePendingOrder(new PendingOrdersToSync
                        {
                            OrderId = order.Id,
                            SuccessDate = DateTime.UtcNow,
                            Synchronized = validOrderSyncNetsuite
                        });

                        
                    }
                }
                else
                {
                    if (order.Customer.Parent != 0)
                        CreateOrderAccountCustomer(order, orderNetsuite);
                    else
                    {
                        //create order if it is paid and if it is web guest or web account customer
                        CreateOrderWebCustomer(order, orderNetsuite);
                    }

                    //validate Order Sync ok 
                    var orderValid = _orderService.GetOrderById(order.Id);
                    bool validOrderSyncNetsuite = false;

                    if (!string.IsNullOrEmpty(orderValid.tranId))
                        validOrderSyncNetsuite = true;
                    
                    _pendingOrdersToSyncService.InsertOrUpdatePendingOrder(new PendingOrdersToSync
                    {
                        OrderId = order.Id,
                        SuccessDate = DateTime.UtcNow,
                        Synchronized = validOrderSyncNetsuite
                    });

                    order.statusNetsuite = 2;
                }

            }
            catch (Exception ex)
            {
                _pendingOrdersToSyncService.InsertOrUpdatePendingOrder(new PendingOrdersToSync
                {
                    OrderId = order.Id,
                    FailedDate = DateTime.UtcNow,
                    Synchronized = false
                });

                order.statusNetsuite = 0;
            }
        }
       
        public void SendOrderNetsuiteEventCreateOrder(Nop.Core.Domain.Orders.Order order)
        {
            try
            {
                _pendingOrdersToSyncService.InsertOrUpdatePendingOrder(new PendingOrdersToSync
                {
                    OrderId = order.Id,
                    FailedDate = DateTime.UtcNow,
                    Synchronized = false
                });
            }
            catch (Exception ex)
            {
                _pendingOrdersToSyncService.InsertOrUpdatePendingOrder(new PendingOrdersToSync
                {
                    OrderId = order.Id,
                    FailedDate = DateTime.UtcNow,
                    Synchronized = false
                });
            }
        }
        private void SendNewOrderNetsuite(Nop.Core.Domain.Orders.Order order, OrderNetsuite orderNetsuite)
        {
            try
            {
                var OrderCompany = order.Customer.Companies.Where(r => r.Id == order.CompanyId).FirstOrDefault();
                var NetsuiteId = "";
                var PrimaryLocation = "";
                if (order.Customer.Companies.Any())
                {
                    if (OrderCompany != null)
                        NetsuiteId = OrderCompany.NetsuiteId;
                    PrimaryLocation = OrderCompany?.PrimaryLocation;
                }
                else
                {
                    NetsuiteId = order.Customer.Companies.Where(r => r.Id == Convert.ToInt32(order.CompanyId)).FirstOrDefault().NetsuiteId;
                    PrimaryLocation = order.Customer.Companies.Where(r => r.Id == Convert.ToInt32(order.CompanyId)).FirstOrDefault().PrimaryLocation;
                }

                if (string.IsNullOrEmpty(NetsuiteId))
                    if (order.CompanyId == 6633)
                        NetsuiteId = "6633";

                orderNetsuite.entity = NetsuiteId;

                Location customForm = new Location();
                customForm.id = _settingService.GetSetting("WebStoreCustomer.customForm").Value; // "198";
                customForm.refName = "Web Order";
                orderNetsuite.customForm = customForm;


                Location payStatus = new Location();
                if (order.PaymentStatus == PaymentStatus.Paid)
                    payStatus.id = "1";
                else
                    payStatus.id = "2";
                orderNetsuite.custbody_website_order_status = payStatus;

                Location Location = new Location();
                Location.refName = PrimaryLocation;
                if (PrimaryLocation != "Atlanta" && PrimaryLocation != "Cincinnati" && PrimaryLocation != "Nashville")
                    Location.refName = "Nashville";

                orderNetsuite.Location = Location;
                if (order.ShippingAddress == null)
                {
                    order.ShippingAddress = order.BillingAddress;
                }


                Models.Order.BillingAddress billingAddress = new Models.Order.BillingAddress();
                billingAddress.first_name = order.BillingAddress?.FirstName;
                billingAddress.last_name = order.BillingAddress?.LastName;
                billingAddress.addr1 = order.BillingAddress?.Address1;
                billingAddress.addr2 = order.ShippingAddress?.Address2;

                if (order.BillingAddress?.PhoneNumber != "0" && order.BillingAddress?.PhoneNumber?.Length > 7)
                    billingAddress.addrphone = order.BillingAddress?.PhoneNumber;

                billingAddress.city = order.BillingAddress?.City;

                Location countryBill = new Location();
                countryBill.id = order.BillingAddress?.Country?.TwoLetterIsoCode;
                countryBill.refName = order.BillingAddress?.Country?.Name;
                billingAddress.country = countryBill;

                billingAddress.state = order.BillingAddress?.StateProvince?.Abbreviation;
                billingAddress.zip = order.BillingAddress?.ZipPostalCode;

                orderNetsuite.billingAddress = billingAddress;
                orderNetsuite.custbody_webstore_email = order.BillingAddress?.Email;

                Models.Order.ShippingAddress shippingAddress = new Models.Order.ShippingAddress();
                shippingAddress.first_name = order.ShippingAddress?.FirstName;
                shippingAddress.last_name = order.ShippingAddress?.LastName;
                shippingAddress.addr1 = order.ShippingAddress?.Address1;
                shippingAddress.addr2 = order.ShippingAddress?.Address2;
                shippingAddress.addressee = order.ShippingAddress?.FirstName + " " + order.ShippingAddress?.LastName;

                if (!string.IsNullOrEmpty(order.ShippingAddress.Company))
                {
                    shippingAddress.attention = order.ShippingAddress.Company;
                }

                if (order.ShippingAddress?.PhoneNumber != "0" && order.ShippingAddress?.PhoneNumber?.Length > 7)
                    shippingAddress.addrphone = order.ShippingAddress?.PhoneNumber;

                shippingAddress.city = order.ShippingAddress?.City;

                Location country = new Location();
                country.id = order.ShippingAddress?.Country?.TwoLetterIsoCode;
                country.refName = order.ShippingAddress?.Country?.Name;
                shippingAddress.country = country;

                shippingAddress.state = order.ShippingAddress?.StateProvince?.Abbreviation;
                shippingAddress.zip = order.ShippingAddress?.ZipPostalCode;

                orderNetsuite.shippingAddress = shippingAddress;


                custbody8 custbody8 = new custbody8();
                custbody8.id = "5";
                orderNetsuite.custbody8 = custbody8;

                shipMethod shipMethod = new shipMethod();
                shipMethod.id = "1915";

                orderNetsuite.shippingCost = order.OrderShippingExclTax;

                if (order.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                {
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingUpsGroup.Name").Value)// "UPS Ground")
                        shipMethod.id = _settingService.GetSetting("ShippingUpsGroup.id").Value; // "1611";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAir.Name").Value)  //"UPS Next Day Air")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAir.id").Value;// "2521";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirEarlyAm.Name").Value)//"UPS Next Day Air Early AM")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value;//"2522";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirSaver.Name").Value)// "UPS Next Day Air Saver")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAirSaver.id").Value;//"2515";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingSecondDay.Name").Value) //"UPS Second Day Air")
                        shipMethod.id = _settingService.GetSetting("ShippingSecondDay.id").Value;// "2520";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingSecondDayAirAm.Name").Value) //"UPS Second Day Air AM")
                        shipMethod.id = _settingService.GetSetting("ShippingSecondDayAirAm.id").Value;//"2513";
                    if (order.ShippingMethod == _settingService.GetSetting("Shipping3DaySelect.Name").Value) //"UPS 3 Day Select")
                        shipMethod.id = _settingService.GetSetting("Shipping3DaySelect.id").Value;//"2514";

                    orderNetsuite.Location.refName = _settingService.GetSetting("ShippingPickupInStore.Nashville").Value;
                    orderNetsuite.Location.id = "";
                }
                if (order.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                {
                    shipMethod.id = _settingService.GetSetting("ShippingPickupInStore.id").Value;//"2697";
                    orderNetsuite.shippingCost = 0;

                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.Norcross").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Atlanta").Value;
                        orderNetsuite.Location = Location;
                    }
                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.OldHickory").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Nashville").Value;
                        orderNetsuite.Location = Location;
                    }
                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.Cincinnati").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Cincinnati").Value;
                        orderNetsuite.Location = Location;
                    }

                    orderNetsuite.Location.refName = Location.refName;
                    orderNetsuite.Location.id = "";
                }
                else
                {

                    if (!string.IsNullOrEmpty(order.WarehouseLocationNN))
                        orderNetsuite.Location.id = order.WarehouseLocationNN;

                }

                var pickupPoints = _shippingService.GetPickupPoints(order.Customer.BillingAddress, order.Customer, storeId: 1);

                if (order.ShippingRateComputationMethodSystemName == "Shipping.NNDelivery" || order.ShippingMethod == "Delivery on N&N Truck")
                {
                    shipMethod.id = _settingService.GetSetting("ShippingNNDelivery.id").Value;  //"2698";
                    orderNetsuite.shippingCost = order.OrderShippingExclTax;

                    if (!string.IsNullOrEmpty(order.ShippingNNDeliverySend))
                    {
                        var NewLocation = _repository.Table.FirstOrDefault(r => r.ValueToSend.ToLower() == order.ShippingNNDeliverySend.ToLower()
                                           );

                        if (NewLocation != null)
                        {
                            Location.refName = NewLocation.Location;
                        }
                    }

                    Location LocationDelivery = new Location();


                    if (pickupPoints != null)
                    {
                        if (order.Customer.NetsuitId != 0)
                        {
                            var pickUpPointNNDelivery = pickupPoints.PickupPoints.Where(r => r.Description == Location.refName).FirstOrDefault();

                            LocationDelivery.id = order.IdShippingNNDeliverySend.ToString();
                            LocationDelivery.refName = order.ShippingNNDeliverySend;

                            ((IDictionary<string, object>)orderNetsuite.DynamicProperties)[pickUpPointNNDelivery.DeliveryRoute] = LocationDelivery;

                        }
                    }

                    if (!string.IsNullOrEmpty(LocationDelivery.refName))
                    {
                        var getLocation = _repository.Table.FirstOrDefault(r => r.ValueToSend.ToLower() == LocationDelivery.refName.ToLower() && r.Location == Location.refName);

                        if (getLocation != null)
                            orderNetsuite.Location.refName = getLocation.Location;
                        else
                        {
                            getLocation = _repository.Table.FirstOrDefault(r => r.ValueToSend.ToLower() == LocationDelivery.refName.ToLower());
                            if (getLocation != null)
                                orderNetsuite.Location.refName = getLocation.Location;

                        }
                    }

                }


                orderNetsuite.shipMethod = shipMethod;
                // orderNetsuite.custbody_tj_external_tax_amount = order.OrderTax;

                orderNetsuite.custbody_taxjar_external_amount_n_n = order.OrderTax;

                orderNetsuite.custbody58 = false;
                orderStatus orderStatus = new orderStatus();
                orderStatus.id = _settingService.GetSetting("NetsuiteOrderStatus.Pending").Value;
                orderNetsuite.orderStatus = orderStatus;
              
                //items 
                Item Item = new Item();
                Item.items = new List<ItemDetail>();
                string discountCode = "";
                string discountCodeSend = "";
                string discountCodeTotalOrder = "";
                foreach (var x in order.OrderItems)
                {
                    Core.Domain.Catalog.Product product = x.Product;
                    decimal price = 0;

                    GetPriceByQtyPricing(order, x, product, out price, out discountCode);
                    if (!string.IsNullOrEmpty(discountCode))
                        discountCodeSend = discountCode;
                    if (product != null)
                    {
                        ItemDetail ItemDetail = new ItemDetail
                        {
                            custcol_item_attribute = x.AttributeDescription,
                            amount = price * x.Quantity,
                            quantity = x.Quantity,
                            rate = price,
                            item = new InventoryItem { id = product.IdInventoryItem }
                        };
                        Item.items.Add(ItemDetail);
                    }

                }

                var IsCommercial = _genericAttributeService.GetAttribute<ShippingOption>(order.Customer,
                                NopCustomerDefaults.SelectedShippingOptionAttributeIsCommercial, _storeContext.CurrentStore.Id);

                var saveaddresstomyaccount = _genericAttributeService.GetAttribute<bool>(order.Customer,
                        NopCustomerDefaults.Saveaddresstomyaccount + "_" + order.Customer.BillingAddressId);

                var SaveShippingaddress = _genericAttributeService.GetAttribute<bool>(order.Customer,
                             NopCustomerDefaults.SaveShippingaddress + "_" + order.Customer.ShippingAddressId);

                var ShipToSameAddress = _genericAttributeService.GetAttribute<bool>(order.Customer,
                        NopCustomerDefaults.ShipToSameAddress + "_" + order.Customer.BillingAddressId);

                bool savedefaultBilling = false;
                bool savedefaultShipping = false;
                bool custrecord_address_pending_approval = false;

                if (saveaddresstomyaccount != null)
                {
                    if (saveaddresstomyaccount)
					{
                        //if(order.Customer.ShippingAddress)
                        var saveBillingaddresstomyaccount = _genericAttributeService.GetAttribute<bool>(order.Customer,
                        NopCustomerDefaults.IsDefaultBilling + "_" + order.Customer.BillingAddressId);

                        if(saveBillingaddresstomyaccount)
						{
                            savedefaultBilling = saveBillingaddresstomyaccount;
                        }
                        custrecord_address_pending_approval = true;

                    }

                }
                if (SaveShippingaddress != null)
                {
                    if (SaveShippingaddress)
                    {
                        var saveShippingaddresstomyaccount = _genericAttributeService.GetAttribute<bool>(order.Customer,
                        NopCustomerDefaults.IsDefaultShipping + "_" + order.Customer.ShippingAddressId);

                        if (saveShippingaddresstomyaccount)
                        {
                            savedefaultShipping = saveShippingaddresstomyaccount;
                        }
                        custrecord_address_pending_approval = true;

                    }

                }

				
                if (saveaddresstomyaccount != null && (saveaddresstomyaccount || SaveShippingaddress))
                {
                    if (ShipToSameAddress)
                    {
                        var addressbook = new Addressbook();
                        var addressbookItems = new itemsList();
                        var AddressDefine = new AddressDefine();

                        var AddressBookAddress = new AddressBookAddress();

                        AddressBookAddress.Country = new EntityRefObject { Id = "US", RefName = "United States" };
                        AddressBookAddress.Zip = order.BillingAddress.ZipPostalCode;
                        AddressBookAddress.Addressee = order.Customer.Companies.Where(r => r.Id == Convert.ToInt32(order.CompanyId)).FirstOrDefault().CompanyName;
                        AddressBookAddress.Addr1 = order.BillingAddress.Address1;
                        AddressBookAddress.City = order.BillingAddress.City;
                        AddressBookAddress.State = order.BillingAddress.StateProvince.Abbreviation;
                        AddressBookAddress.custrecord_address_pending_approval = custrecord_address_pending_approval;
                        AddressBookAddress.Addr2 = order.BillingAddress.Address2;
                        AddressBookAddress.AddrPhone = order.BillingAddress.PhoneNumber;
                        AddressBookAddress.attention = order.BillingAddress.FirstName + " " + order.ShippingAddress.LastName;

                        AddressDefine.addressbookaddress = new AddressBookAddress();
                        AddressDefine.addressbookaddress = AddressBookAddress;

                        AddressDefine.defaultBilling = savedefaultBilling;
                        AddressDefine.defaultShipping = savedefaultBilling;

                        addressbookItems.items = new List<AddressDefine>();
                        addressbookItems.items.Add(AddressDefine);

                        addressbook.addressbook = new itemsList();

                        addressbook.addressbook = addressbookItems;


                        AddAddressCustomer(orderNetsuite.entity, addressbook);
                    }
					else
					{
						if (saveaddresstomyaccount)
						{
                            var addressbook = new Addressbook();
                            var addressbookItems = new itemsList();
                            var AddressDefine = new AddressDefine();

                            var AddressBookAddress = new AddressBookAddress();

                            AddressBookAddress.Country = new EntityRefObject { Id = "US", RefName = "United States" };
                            AddressBookAddress.Zip = order.BillingAddress.ZipPostalCode;
                            AddressBookAddress.Addressee = order.Customer.Companies.Where(r => r.Id == Convert.ToInt32(order.CompanyId)).FirstOrDefault().CompanyName;
                            AddressBookAddress.Addr1 = order.BillingAddress.Address1;
                            AddressBookAddress.City = order.BillingAddress.City;
                            AddressBookAddress.State = order.BillingAddress.StateProvince.Abbreviation;
                            AddressBookAddress.custrecord_address_pending_approval = custrecord_address_pending_approval;
                            AddressBookAddress.Addr2 = order.BillingAddress.Address2;
                            AddressBookAddress.AddrPhone = order.BillingAddress.PhoneNumber;
                            AddressBookAddress.attention = order.BillingAddress.FirstName + " " + order.ShippingAddress.LastName;

                            AddressDefine.addressbookaddress = new AddressBookAddress();
                            AddressDefine.addressbookaddress = AddressBookAddress;

                            AddressDefine.defaultBilling = savedefaultBilling;
                            //AddressDefine.defaultShipping = savedefaultShipping;

                            addressbookItems.items = new List<AddressDefine>();
                            addressbookItems.items.Add(AddressDefine);

                            addressbook.addressbook = new itemsList();

                            addressbook.addressbook = addressbookItems;


                            AddAddressCustomer(orderNetsuite.entity, addressbook);
                        }

                        if (SaveShippingaddress)
                        {
                            var addressbook = new Addressbook();
                            var addressbookItems = new itemsList();
                            var AddressDefine = new AddressDefine();

                            var AddressBookAddress = new AddressBookAddress();

                            AddressBookAddress.Country = new EntityRefObject { Id = "US", RefName = "United States" };
                            AddressBookAddress.Zip = order.ShippingAddress.ZipPostalCode;
                            AddressBookAddress.Addr2 = order.ShippingAddress.Address2;
                            AddressBookAddress.AddrPhone = order.ShippingAddress.PhoneNumber;
                            AddressBookAddress.attention = order.ShippingAddress.FirstName+" " + order.ShippingAddress.LastName;

                            AddressBookAddress.Addressee = order.Customer.Companies.Where(r => r.Id == Convert.ToInt32(order.CompanyId)).FirstOrDefault().CompanyName;
                            AddressBookAddress.Addr1 = order.ShippingAddress.Address1;
                            AddressBookAddress.City = order.ShippingAddress.City;
                            AddressBookAddress.State = order.ShippingAddress.StateProvince.Abbreviation;
                            AddressBookAddress.custrecord_address_pending_approval = custrecord_address_pending_approval;

                            AddressDefine.addressbookaddress = new AddressBookAddress();
                            AddressDefine.addressbookaddress = AddressBookAddress;

                            //AddressDefine.defaultBilling = savedefaultBilling;
                            AddressDefine.defaultShipping = savedefaultShipping;

                            addressbookItems.items = new List<AddressDefine>();
                            addressbookItems.items.Add(AddressDefine);

                            addressbook.addressbook = new itemsList();

                            addressbook.addressbook = addressbookItems;


                            AddAddressCustomer(orderNetsuite.entity, addressbook);
                        }
                    }
                    
                }
                
                if (IsCommercial != null)
                {
                    if (IsCommercial.IsCommercial)
                        orderNetsuite.shipIsResidential = false;
                    else
                        orderNetsuite.shipIsResidential = true;


                }

                

                orderNetsuite.item = Item;
                orderNetsuite.custbody_web_address = order.Id.ToString();
                orderNetsuite.custbody_website_order_number = ModeAbb + order.Id.ToString();
                orderNetsuite.otherRefNum = order.PO;
                if (string.IsNullOrEmpty(order.PO))
                    orderNetsuite.otherRefNum = "";

                if (order.DiscountUsageHistory.Count() > 0)
                {
                    foreach (var item in order.DiscountUsageHistory)
                    {
                        if (item.Discount?.CouponCode != null && item.Discount?.CouponCode != "TestDiscount")
                            discountCodeTotalOrder = item.Discount.CouponCode;
                    }
                }
                if (order.Customer.Parent != 0)
                    orderNetsuite.custbody_website_user_role = "Account Customer";
                
                orderNetsuite.custbody_promotion = discountCodeTotalOrder;
                orderNetsuite.custbody_pickup_person_notes = order.PickupPersonNote;

                // orderNetsuite.DynamicProperties = null;

                var jsonString = JsonConvert.SerializeObject(orderNetsuite);
                JObject jsonObject = JObject.Parse(jsonString);
                Location LocationDeliveryRoute= new Location();

                if (order.ShippingRateComputationMethodSystemName == "Shipping.NNDelivery" || order.ShippingMethod == "Delivery on N&N Truck")
                {
                    if (pickupPoints != null)
                    {
                        if (order.Customer.NetsuitId != 0)
                        {
                            var pickUpPointNNDelivery2 = pickupPoints.PickupPoints.Where(r => r.Description == Location.refName).FirstOrDefault();

                            LocationDeliveryRoute.id = order.IdShippingNNDeliverySend.ToString();
                            LocationDeliveryRoute.refName = order.ShippingNNDeliverySend;

                            ((IDictionary<string, object>)orderNetsuite.DynamicProperties)[pickUpPointNNDelivery2.DeliveryRoute] = LocationDeliveryRoute;

                            JObject custbodyDeliveryRoute = new JObject
                            {
                                { "id", LocationDeliveryRoute.id },
                                { "refName", LocationDeliveryRoute.refName  }
                            };

                            jsonObject.Add(pickUpPointNNDelivery2.DeliveryRoute, custbodyDeliveryRoute);
                        }
                    }
                }
                _logger.Warning("ImportSendOrderLog:: Before Send New Order to Netsuite:: order Id" + order.Id+ "LastExecutionDateGeneral:: " + DateTime.Now);

                // Serialize the updated JSON object back to a JSON string
                string updatedJsonString = jsonObject.ToString();


                CreateOrder(updatedJsonString);

                _logger.Warning("ImportSendOrderLog:: After Send New Order to Netsuite:: order Id" + order.Id + "LastExecutionDateGeneral:: " + DateTime.Now);


                //validate Order In netsuite 
                if (NetsuiteId!=null)
                {
                    var OrderUpdate = GetOrderbyNetsuite(Convert.ToInt32(NetsuiteId), order.Id);

                    if (OrderUpdate != null)
                    {
                        var orderInser = OrderUpdate.Items.Where(r => r.OrderNopId == ModeAbb + order.Id.ToString()).FirstOrDefault();
                        if (orderInser != null)
                        {
                            if (order.CustomerIp != "0")
                            {
                                order.Source = "REST Web Services";
                            }

                            order.tranId = orderInser.Document;
                            _logger.Warning("ImportSendOrderLog:: Before Update transId From New Order to Netsuite:: order Id" + order.Id + "LastExecutionDateGeneral:: " + DateTime.Now);

                            _orderService.UpdateOrder(order);
                            _logger.Warning("ImportSendOrderLog:: After Update transId From New Order to Netsuite:: order Id" + order.Id + "LastExecutionDateGeneral:: " + DateTime.Now);

                        }

                    }
                }

            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportSendOrderError:: Send New Order to Netsuite:: order Id" + order.Id + "  order Netsuite Id " + order.tranId +" Customer Id" + order.Id+ " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private bool SendUpdateOrderNetsuite(Nop.Core.Domain.Orders.Order order, TransactionDto orderNetsuiteUpdate)
        {

            OrderUpdateNetsuite orderNetsuite = new OrderUpdateNetsuite();
            var NetsuiteId = "";
            var PrimaryLocation = "";
            shipAddressList ShipList = new shipAddressList();
            bool update = false;

            try
            {
                if (order.Customer.Parent != 0)
                {
                    var OrderCompany = order.Customer.Companies.Where(r => r.Id == order.CompanyId).FirstOrDefault();

                    if (order.Customer.Companies.Any())
                    {
                        if (OrderCompany != null)
                            NetsuiteId = OrderCompany.NetsuiteId;
                        PrimaryLocation = OrderCompany?.PrimaryLocation;
                    }
                    else
                    {
                        NetsuiteId = order.Customer.Companies.Where(r => r.Id == Convert.ToInt32(order.CompanyId)).FirstOrDefault().NetsuiteId;
                        PrimaryLocation = order.Customer.Companies.Where(r => r.Id == Convert.ToInt32(order.CompanyId)).FirstOrDefault().PrimaryLocation;
                    }
                    ShipList.id = "6458";
                }
                else
                {
                    NetsuiteId = _settingService.GetSetting("WebStoreCustomer.id").Value;
                    PrimaryLocation = _settingService.GetSetting("WebStoreCustomer.PrimaryLocation").Value;
                    ShipList.id = "69313";
                }


                orderNetsuite.id = orderNetsuiteUpdate.Orderid.ToString();
                //orderNetsuite.custbody31 = order.Id.ToString();
                Location customForm = new Location();
                customForm.id = _settingService.GetSetting("WebStoreCustomer.customForm").Value; // "198";
                customForm.refName = "Web Order";
                orderNetsuite.customForm = customForm;

                // orderNetsuite.otherRefNum = order.Id;
                Location payStatus = new Location();
                if (order.PaymentStatus == PaymentStatus.Paid)
                    payStatus.id = "1";
                else
                    payStatus.id = "2";

                orderNetsuite.custbody_website_order_status = payStatus;

                Location Location = new Location();
                Location.refName = PrimaryLocation;
                orderNetsuite.Location = Location;
                if (order.ShippingAddress == null)
                {
                    order.ShippingAddress = order.BillingAddress;
                }

                custbody8 custbody8 = new custbody8();
                custbody8.id = "5";
                orderNetsuite.custbody8 = custbody8;

                shipMethod shipMethod = new shipMethod();
                shipMethod.id = "1915";


                if (order.ShippingRateComputationMethodSystemName == "Shipping.UPS")
                {
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingUpsGroup.Name").Value)// "UPS Ground")
                        shipMethod.id = _settingService.GetSetting("ShippingUpsGroup.id").Value; // "1611";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAir.Name").Value)  //"UPS Next Day Air")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAir.id").Value;// "2521";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirEarlyAm.Name").Value)//"UPS Next Day Air Early AM")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value;//"2522";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirSaver.Name").Value)// "UPS Next Day Air Saver")
                        shipMethod.id = _settingService.GetSetting("ShippingNextDayAirSaver.id").Value;//"2515";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingSecondDay.Name").Value) //"UPS Second Day Air")
                        shipMethod.id = _settingService.GetSetting("ShippingSecondDay.id").Value;// "2520";
                    if (order.ShippingMethod == _settingService.GetSetting("ShippingSecondDayAirAm.Name").Value) //"UPS Second Day Air AM")
                        shipMethod.id = _settingService.GetSetting("ShippingSecondDayAirAm.id").Value;//"2513";
                    if (order.ShippingMethod == _settingService.GetSetting("Shipping3DaySelect.Name").Value) //"UPS 3 Day Select")
                        shipMethod.id = _settingService.GetSetting("Shipping3DaySelect.id").Value;//"2514";

                    orderNetsuite.shippingCost = order.OrderShippingExclTax;
                    orderNetsuite.Location.refName = _settingService.GetSetting("ShippingPickupInStore.Nashville").Value;
                }
                if (order.ShippingRateComputationMethodSystemName == "Pickup.PickupInStore")
                {
                    shipMethod.id = _settingService.GetSetting("ShippingPickupInStore.id").Value;//"2697";
                    orderNetsuite.shippingCost = 0;

                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.Norcross").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Atlanta").Value;
                        orderNetsuite.Location = Location;
                    }
                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.OldHickory").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Nashville").Value;
                        orderNetsuite.Location = Location;
                    }
                    if (order.PickupAddress?.City == _settingService.GetSetting("ShippingPickupInStore.Cincinnati").Value)
                    {
                        Location.refName = _settingService.GetSetting("ShippingPickupInStore.Cincinnati").Value;
                        orderNetsuite.Location = Location;
                    }

                    orderNetsuite.Location.refName = Location.refName;
                }
                if (order.ShippingRateComputationMethodSystemName == "Shipping.NNDelivery")
                {
                    shipMethod.id = _settingService.GetSetting("ShippingNNDelivery.id").Value;  //"2698";
                    orderNetsuite.shippingCost = order.OrderShippingInclTax;

                    if (!string.IsNullOrEmpty(order.ShippingNNDeliverySend))
                    {
                        var NewLocation = _repository.Table.FirstOrDefault(r => r.ValueToSend.ToLower() == order.ShippingNNDeliverySend.ToLower()
                                           && r.Id == order.IdShippingNNDeliverySend);

                        if (NewLocation != null)
                        {
                            Location.refName = NewLocation.Location;
                        }
                    }
                    Location LocationDelivery = new Location();

                    if (Location.refName == _settingService.GetSetting("deliverysettings.restrictedcityes.nashville.nashville").Value)
                    {
                        LocationDelivery.id = order.IdShippingNNDeliverySend.ToString();
                        LocationDelivery.refName = order.ShippingNNDeliverySend;
                    }

                    if (Location.refName == _settingService.GetSetting("deliverysettings.restrictedcityes.atlanta.atlanta").Value)
                    {
                        LocationDelivery.id = order.IdShippingNNDeliverySend.ToString();
                        LocationDelivery.refName = order.ShippingNNDeliverySend;
                    }

                    if (Location.refName == _settingService.GetSetting("deliverysettings.restrictedcityes.cincinnati.cincinnati").Value)
                    {
                        LocationDelivery.id = order.IdShippingNNDeliverySend.ToString();
                        LocationDelivery.refName = order.ShippingNNDeliverySend;
                    }

                    if (!string.IsNullOrEmpty(LocationDelivery.refName))
                    {
                        var getLocation = _repository.Table.FirstOrDefault(r => r.ValueToSend.ToLower() == LocationDelivery.refName.ToLower());

                        if (getLocation != null)
                            orderNetsuite.Location.refName = getLocation.Location;
                    }
                    //    if(order.ShippingAddress?.City==)
                }

                orderNetsuite.shipMethod = shipMethod;
                orderNetsuite.custbody58 = false;

              
                //orderNetsuite.custbody_tj_external_tax_amount = order.OrderTax;
                orderNetsuite.custbody_taxjar_external_amount_n_n = order.OrderTax;

                var request = UpdateOrderNetsuite(orderNetsuite, orderNetsuite.id);

                if (request == "NoContent")
                    update = true;

                if (string.IsNullOrEmpty(order.tranId) && !string.IsNullOrEmpty(orderNetsuiteUpdate.Document))
                {
                    order.tranId = orderNetsuiteUpdate.Document;
                    _orderService.UpdateOrder(order);
                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportSendOrderError:: SendUpdateOrderNetsuite GetPriceByQtyPricing:: order Id" + order.Id + "  order Netsuite Id " + order.tranId + " Customer Id" + order.Id + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
            return update;
        }

        private void UpdateOrderIdNopCommerceToNetsuite(Nop.Core.Domain.Orders.Order order, OrderDto orderDto)
        {
            OrderUpdateNetsuiteOrderId orderNetsuite = new OrderUpdateNetsuiteOrderId();
            orderNetsuite.custbody_website_order_number = ModeAbb + order.Id.ToString();

            UpdateOrderNetsuiteOrderId(orderNetsuite, orderDto.id);
        }

        #endregion

        #endregion

        #region Method Netsuite

        private void AddAddressCustomer(string customerId, Addressbook addressbook)
        {
            var jsonString = JsonConvert.SerializeObject(addressbook);
            var data = _connectionService.GetConnection("/record/v1/customer/"+ customerId, "PATCH", jsonString);
        }

        private OrderDto GetOrderId(int id)
        {
            var data = _connectionService.GetConnection(string.Concat("/record/v1/salesOrder/", id.ToString()), "GET");
            return JsonConvert.DeserializeObject<OrderDto>(data);
        }

        private OrderItemListDto GetItemOrderListId(int orderId)
        {
            var data = _connectionService.GetConnection(string.Concat("/record/v1/salesOrder/", orderId.ToString(), "/item"), "GET");
            return JsonConvert.DeserializeObject<OrderItemListDto>(data);
        }

        private OrderItemDto GetItemOrderId(int orderId, string orderItemId)
        {
            var data = _connectionService.GetConnection(string.Concat("/record/v1/salesOrder/", orderId.ToString(), "/item/", orderItemId), "GET");
            return JsonConvert.DeserializeObject<OrderItemDto>(data);
        }

        private TransactionListDto GetPendingSalesbyNetsuite(int customerId, string LastExecutionDate=null)
        {
            string LastModified = "";
            //if (LastExecutionDate!=null)
            //    LastModified= " and t.lastmodifieddate >='"+LastExecutionDate+"'";

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10, t.custbody_website_order_number, t.lastmodifieddate   FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'  and t.status='A' and c.id=" + customerId + LastModified+" \"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetPendingSalesbyNetsuiteWebAccount(int webAccountNetsuite,  string orderId, string LastExecutionDate, string delimit)
        {
            string LastModified = "";
            //if (LastExecutionDate != null)
            //    LastModified = " and t.lastmodifieddate >='" + LastExecutionDate + "'";

            string order = "";
            if (orderId!=null)
                order = " and t.custbody_website_order_number="+ModeAbb + orderId;

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10, t.custbody_website_order_number, t.status,t.custbody_website_order_status  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'and t.status='A'  and c.id=" + webAccountNetsuite + order + LastModified +"  \"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql" + delimit, "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }
        private TransactionListDto GetOpenSalesbyNetsuite(int customerId, string LastExecutionDate)
        {
            string LastModified = "";
            //if (LastExecutionDate != null)
            //    LastModified = " and t.lastmodifieddate >='" + LastExecutionDate + "'";

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody_website_order_number, t.custbody10  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'  and t.status='B' and c.id=" + customerId + LastModified +"\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetCancelledClosedOrdersbyNetsuite(int customerId, string LastExecutionDate)
        {
            string LastModified = "";
            //if (LastExecutionDate != null)
            //    LastModified = " and t.lastmodifieddate >='" + LastExecutionDate + "'";

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody_website_order_number, t.custbody10  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'  and (t.status='H' or t.status='C') and c.id=" + customerId + LastModified + "\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetPendingBillingSalesbyNetsuite(int customerId, string LastExecutionDate)
        {
            string LastModified = "";
            //if (LastExecutionDate != null)
            //    LastModified = " and t.lastmodifieddate >='" + LastExecutionDate + "'";

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'  and t.status='F' and c.id=" + customerId + LastModified +"\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetPendingBillingPartiallyFulfilledbyNetsuite(int customerId, string LastExecutionDate)
        {
            string LastModified = "";
            //if (LastExecutionDate != null)
            //    LastModified = " and t.lastmodifieddate >='" + LastExecutionDate + "'";

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'  and t.status='E' and c.id=" + customerId + LastModified +"\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetBilledbyNetsuite(int customerId, string LastExecutionDate)
        {
            string LastModified = "";
            if (LastExecutionDate != null)
                LastModified = " and t.lastmodifieddate >='" + LastExecutionDate + "'";

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'  and t.status='G' and c.id=" + customerId + LastModified + "\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetPartiallyFulfilledbyNetsuite(int customerId, string LastExecutionDate)
        {
            string LastModified = "";
            //if (LastExecutionDate != null)
            //    LastModified = " and t.lastmodifieddate >='" + LastExecutionDate + "'";

            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd'  and t.status='D' and c.id=" + customerId + LastModified + "\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetOpenSalesbyNetsuiteLastUpdate( string LastExecutionDate)
        {
            var body = @"{""q"": ""SELECT t.lastmodifieddate as transDate, c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10, t.*  FROM customer c, transaction t WHERE t.entity = c.id  AND t.type = 'SalesOrd' and t.lastmodifieddate>='" + LastExecutionDate + "'\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }


        private TransactionListDto GetOrderbyNetsuite(int customerId, int orderIdNop)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId, t.custbody10,t.custbody_website_order_number  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'SalesOrd' and  t.custbody_website_order_number='" + ModeAbb + orderIdNop + "' and  c.id=" + customerId + "\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetTransactionCashSales(string orderId, int customerId)
        {
            var body = @"{""q"": "" SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId,  t.status ,  t.createddate, t.foreigntotal, t.tranid, t.status, t.lastmodifieddate, t.custbody10 FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CashSale' and c.id=" + customerId + "  and  t.custbody_website_order_number='" + ModeAbb + orderId + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetTransactionCashSalesOrderId(string orderId, int customerId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId,  t.status ,  t.createddate, t.foreigntotal, t.tranid, t.status, t.lastmodifieddate, t.custbody10,t.custbody_website_order_number FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CashSale' and c.id=" + customerId + "  and  t.custbody_website_order_number=''" + ModeAbb + orderId + "''\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetTransactionInvoice(string orderId, int customerId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId,  t.status ,  t.createddate, t.foreigntotal, t.tranid, t.status, t.lastmodifieddate, t.custbody10, t.duedate FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustInvc' and c.id=" + customerId + "  and  t.custbody_website_order_number='" + ModeAbb + orderId + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetTransactionInvoiceId(string Id)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId,  t.status ,  t.createddate, t.foreigntotal, t.tranid, t.status, t.lastmodifieddate, t.custbody10, t.duedate, t.foreignamountunpaid, t.foreignamountpaid  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustInvc' and t.id=" + Id + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }
        private TransactionListDto GetTransactionInvoiceIdByCompany(string Id, int customerId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId,  t.status ,  t.createddate, t.foreigntotal, t.tranid, t.status, t.lastmodifieddate, t.custbody10, t.duedate, t.foreignamountunpaid, t.foreignamountpaid  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustInvc'  AND t.tranId='" + Id + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }
        private TransactionListDto GetTransactionInvoiceByCompanyId(int customerId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId,  t.status ,  t.createddate, t.foreigntotal, t.tranid, t.status, t.lastmodifieddate, t.custbody10, t.duedate FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustInvc' and c.id=" + customerId + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetTransactionInvoiceOrderId(string orderId, int customerId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS OrderId,  t.status ,  t.createddate, t.foreigntotal, t.tranid, t.status, t.lastmodifieddate, t.custbody10,t.custbody_website_order_number FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustInvc' and c.id=" + customerId + "  and  t.custbody_website_order_number=''" + ModeAbb + orderId + "''\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        
        private TransactionsInfoDto GetTransactionOrderStatus(string orderId)
        {
            var body = @"{""q"": ""SELECT t.transaction, t.linesequencenumber  FROM  transactionline t WHERE t.createdfrom='" + orderId + "' order by t.linesequencenumber desc \" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionsInfoDto>(data);
        }

        private ItemFullfitmentDto GetitemFulfillmentNetsuite(string itemFulfillmentId)
        {
            var data = _connectionService.GetConnection("/record/v1/itemFulfillment/" + itemFulfillmentId, "GET");
            return JsonConvert.DeserializeObject<ItemFullfitmentDto>(data);
        }

        private string UpdateOrderNetsuite(OrderUpdateNetsuite order, string OrderId)
        {
            var jsonString = JsonConvert.SerializeObject(order);
            
            var data = _connectionService.GetConnection("/record/v1/salesOrder/" + OrderId, "PATCH", jsonString);

            return data;
        }
        private string UpdateOrderNetsuiteOrderId(OrderUpdateNetsuiteOrderId order, string OrderId)
        {
            var jsonString = JsonConvert.SerializeObject(order);
             var data = _connectionService.GetConnection("/record/v1/salesOrder/" + OrderId, "PATCH", jsonString);

            return data;
        }

        private InvoiceDto GetInvoiceFromNetsuite(string IdInvoice)
        {
            var data = _connectionService.GetConnection("/record/v1/invoice/" + IdInvoice, "GET");
            return JsonConvert.DeserializeObject<InvoiceDto>(data);
        }

        private void CreateOrder(OrderNetsuite order)
        {
            var jsonString = JsonConvert.SerializeObject(order);
            var data = _connectionService.GetConnection("/record/v1/salesOrder", "POST", jsonString);
        }

        private void CreateOrder(string jsonString)
        {
             _connectionService.GetConnection("/record/v1/salesOrder", "POST", jsonString);
        }

        private void CreatePayment(PaytmentDto payment)
        {
            var jsonString = JsonConvert.SerializeObject(payment);
            var data = _connectionService.GetConnection("/record/v1/customerpayment", "POST", jsonString);
        }

        private void CreatePayment(PaytmentDto payment, int paymentId)
        {
            var jsonString = JsonConvert.SerializeObject(payment);
            var data = _connectionService.GetConnection("/record/v1/customerpayment/"+ paymentId, "PATCH", jsonString);
        }
        #endregion

        #region Other Methods

        public void SyncPendingOrders(string LastExecutionDateGeneral=null)
        {
            var pendingOrders = _pendingOrdersToSyncService.GetAllPendingOrders();

            foreach (var pendingOrder in pendingOrders)
			{
				CreateOrderInNetsuite(LastExecutionDateGeneral, pendingOrder.OrderId);

			}
		}

		public void CreateOrderInNetsuite(string LastExecutionDateGeneral, int OrderId)
		{
			var order = _orderService.GetOrderById(OrderId);
			if (order != null && !order.Deleted)
			{
				if (order.statusNetsuite != 1)
				{
					order.statusNetsuite = 1;
					_orderService.UpdateOrder(order);
					SendOrderNetsuite(order);
				}
				else
				{
					var dateExecution = Convert.ToDateTime(LastExecutionDateGeneral);
					var newDate = DateTime.Now.AddHours(3);
					_logger.Information("send Pending orders: Last dateExecution" + dateExecution.ToString() + "-- current hour:" + newDate.ToString());

					if (dateExecution < newDate)
					{
						_logger.Information("send Pending orders:" + order.Id);

						order.statusNetsuite = 1;
						SendOrderNetsuite(order);
					}
				}
			}
		}

		#endregion

		#region Invoice

		public void GetOpenInvoiceByCompanyId(int companyNetsuiteId, int companyId)
        {
            var Transaction = GetTransactionInvoiceByCompanyId(companyNetsuiteId);
            var companyIdNp = _companyService.GetCompanyByNetSuiteId(companyNetsuiteId).FirstOrDefault();
            try
            {
                    var json = JsonConvert.SerializeObject(Transaction);
                    if (Transaction != null)
                    {
                        foreach (var item in Transaction.Items)
                        {
                            if (item.Document == "INV186130" || item.Document == "INV186228" || item.Document == "INV186275" || item.Document == "INV186604")
                            {
                            }
                            UpdateOrderFromNetsuiteNopcommerce(companyIdNp.Id, item);
                        }
                    }

                    // Get credits, payments
                    _creditManageService.GetCustomerBalance(companyNetsuiteId, null);

                
            }

            catch (Exception ex)
            {
                if(companyIdNp==null)                
                    _logger.Error("GetOpenInvoiceByCompanyId that company not exit: "+ companyNetsuiteId);
                else
                    _logger.Error("GetOpenInvoiceByCompanyId "+ companyNetsuiteId, ex);

                throw new ArgumentNullException();
            }
        }

        #endregion

        #region API Update order from Netsuite

        private void UpdateOrderFromApi(int orderId, string shippingStatus)
        {

            try
            {
                if (orderId != 0)
                {
                    var orderNetsuite = GetOrderId(orderId);
                    if (orderNetsuite != null)
                    {
                        var order = _orderService.GetOrderByNetsuiteId(orderNetsuite.tranId);

                        if (order != null)
						{

							order.OrderTotal = Convert.ToDecimal(orderNetsuite.total);
							order.OrderSubtotalInclTax = Convert.ToDecimal(orderNetsuite.subtotal);
							order.OrderSubtotalExclTax = Convert.ToDecimal(orderNetsuite.subtotal);
							order.OrderShippingInclTax = Convert.ToDecimal(orderNetsuite.shippingCost);
							order.OrderStatusId = GetOrderStatus(orderNetsuite.status?.id);
							order.NNDeliveryDate = orderNetsuite.custbody34;

							if (orderNetsuite.custbody10 != null)
							{
								order.ShippingMethod = orderNetsuite.custbody10?.refName;
							}
                            var orderstatus = order.OrderStatus;


                            OrderStatusFromNetsuite(order, orderNetsuite.status?.id);

							if(shippingStatus!=null)
							    GetShippingStatusValue(order, shippingStatus);
							GetShippingMethod(order, orderNetsuite.shipMethod?.id, orderNetsuite.shipMethod?.refName);
							ValidateShippingMethodUps(order, orderNetsuite);
							ValidateShippingMethodStorePickUpNNDelivery(order, orderNetsuite);
							PaymentStatusFromNetsuite(orderNetsuite, order);

							var items = GetOrderItems(orderId);

							if (items.Count() > 0)
							{
								UpdateOrderItemsFromNetsuite(items, order);
							}

							AddUpdateInvoiceFromNetsuite(orderId, order, order.Customer.NetsuitId);

							_orderService.UpdateOrder(order);

                            UpdateOrder(order, orderId.ToString());

                        }
						else
						{

							var companyInfo = _companyService.GetCompanyByNetSuiteId(Convert.ToInt32(orderNetsuite.entity?.id)).FirstOrDefault();
							if (companyInfo != null)
							{
								var customer = companyInfo.Customers.FirstOrDefault();

								if (customer != null)
								{
									var items = GetOrderItems(orderId);
									InsertOrder(orderNetsuite, items, customer);

									if (orderNetsuite.custbody_website_order_number == null)
									{
										order = _orderService.GetOrderByNetsuiteId(orderNetsuite.tranId);
										UpdateOrderIdNopCommerceToNetsuite(order, orderNetsuite);
									}

									AddUpdateInvoiceFromNetsuite(orderId, order, customer.NetsuitId);
                                    UpdateOrder(order, orderId.ToString());
                                }
							}

						}

                       
                    }

                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportOrderError:: UpdateOrderFromApi  ::  Order id " + orderId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

            }
        }

		private static void PaymentStatusFromNetsuite(OrderDto orderNetsuite, Core.Domain.Orders.Order order)
		{
			if (orderNetsuite.orderStatus?.id == "G")
				order.PaymentStatus = PaymentStatus.Paid;
			else
				 if (order.PaymentStatus != PaymentStatus.Paid)
				order.PaymentStatus = GetPaymentStatusFromNetsuite(orderNetsuite.custbody_website_order_status?.id.ToString());
		}

		private void AddUpdateInvoiceFromNetsuite(int orderId, Core.Domain.Orders.Order order, int NetsuitId)
		{
			var Transaction = GetTransactionInvoiceOrderId(orderId.ToString(), NetsuitId);

			if (Transaction != null)
			{
				var trans = Transaction.Items.FirstOrDefault();
				if (trans != null)
					UpdateOrderFromNetsuiteNopcommerce(null, order, NetsuitId, trans);
			}
		}
        #endregion


        #region Invoices

        private void SendInvoicesPaymentToNetsuite(int transactionId)
        {

      //      try
      //      {
      //          if (transactionId != 0)
      //          {
      //              var transaction = _invoice.GetTransactionById(transactionId);

      //              if (transaction != null)
      //              {
      //                  List<InvoicesToPay> listInvoices = JsonConvert.DeserializeObject<List<InvoicesToPay>>(transaction.InvoiceApply);
      //                  List<CustomerAccountCredit> listCredits = JsonConvert.DeserializeObject<List<CustomerAccountCredit>>(transaction.CustomerDepositeApply);

      //                  PaytmentDto paytmentDto = new PaytmentDto();

      //                  var totalCredits = listCredits.Sum(r => r.TotalApply);
      //                  paytmentDto.payment = transaction.TotalCreditCardPay; //+ totalCredits;
      //                  paytmentDto.paymentoption = 39613;

      //                  ItemsDto invoicesitemsDto = new ItemsDto();

      //                  foreach (var item in listInvoices)
      //                  {
      //                      if (item.OrderId != null)
      //                      {
      //                          var invoice = _invoice.GetInvoiceById(Convert.ToInt32(item.OrderId));

      //                          if (invoice != null && invoice.InvoiceNetSuiteId == 0)
      //                          {
      //                              var invoiceNetsuite = GetTransactionInvoiceIdByCompany(invoice.InvoiceNo, listCredits.FirstOrDefault().CompanyId);

      //                              if (invoiceNetsuite != null && invoiceNetsuite.Items.Count() > 0)
      //                              {
      //                                  var invoiceId = Convert.ToInt32(invoiceNetsuite.Items.FirstOrDefault()?.Orderid);

      //                                  if (invoiceId != 0 && invoiceId != invoice.InvoiceNetSuiteId)
      //                                  {
      //                                      invoice.InvoiceNetSuiteId = invoiceId;

      //                                      _invoice.UpdateOrder(invoice);
      //                                  }
      //                              }
      //                          }

      //                          if (invoice != null)
      //                          {
      //                              ItemDto itemDto = new ItemDto();

      //                              itemDto.amount = Convert.ToDecimal(item.ValuePay);
      //                              itemDto.apply = true;
      //                              itemDto.line = 0;
      //                              itemDto.doc.id = invoice.InvoiceNetSuiteId; // Convert id to string
      //                              invoicesitemsDto.items.Add(itemDto);
      //                          }
      //                      }
      //                  }

      //                  if (invoicesitemsDto.items.Count > 0)
      //                      paytmentDto.apply = invoicesitemsDto;


      //                  ItemsDto creditDto = new ItemsDto();
      //                  ItemsDto paymentsDto = new ItemsDto();

      //                  foreach (var item in listCredits)
      //                  {
      //                      ItemDto itemDto = new ItemDto();
      //                      itemDto.amount = item.TotalApply;
      //                      itemDto.apply = true;
      //                      itemDto.line = 0;
      //                      itemDto.doc.id = item.NetsuiteId; // Convert id to string

      //                      if (item.Type == 3)
						//	{
      //                          paymentsDto.items.Add(itemDto);
      //                      }
      //                      else
      //                      {
      //                          creditDto.items.Add(itemDto);
      //                      }
      //                  }

      //                  if (creditDto.items.Count > 0)
      //                      paytmentDto.credit =creditDto;

                      
      //                  paytmentDto.customer.id = listCredits.FirstOrDefault()?.CompanyId ?? 0; // Set default value if null

      //                  if (creditDto.items.Count > 0)
      //                  {
      //                      CreatePayment(paytmentDto);
      //                  }

                        

      //                  if (paymentsDto.items.Count > 0)
						//{

      //                      foreach (var item in paymentsDto.items)
      //                      {
      //                          paytmentDto.payment = item.amount;

      //                          CreatePayment(paytmentDto, item.doc.id);

      //                      }
      //                  }


                         
      //              }

      //          }
      //      }
      //      catch (Exception ex)
      //      {
      //          _notificationService.ErrorNotification(ex.Message);
      //          _logger.Warning("ImportOrderError:: UpdateOrderFromApi  ::  Order id " + transactionId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

      //      }
        }
        #endregion
    }
}
