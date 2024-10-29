using Nop.Core.Data;
using Nop.Core.Domain.NN;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Media;
using Nop.Services.Logging;
using Nop.Services.NN;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public class ImportManagerService : IImportManagerService
    {
        #region Fields

        private readonly IRepository<NetSuiteImporter> _repositoryNetSuiteImporter;
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IOrderService _orderService;
        private readonly IS3Service _iS3Service;
        private IPendingDataToSyncService _pendingDataToSyncService;
        private readonly ILogger _logger;

        #endregion

        #region Ctor

        public ImportManagerService(IRepository<NetSuiteImporter> repositoryNetSuiteImporter,
            IProductService productService,
            ICustomerService customerService,
            IOrderService orderService,
            IS3Service iS3Service, IPendingDataToSyncService pendingDataToSyncService, ILogger logger)
        {
            _repositoryNetSuiteImporter = repositoryNetSuiteImporter;
            _productService = productService;
            _customerService = customerService;
            _orderService = orderService;
            _iS3Service = iS3Service;
            _pendingDataToSyncService = pendingDataToSyncService;
            _logger = logger;
        }

        #endregion

        #region Methods

        public NetSuiteImporter GetImporter(ImporterIdentifier identifier, string idcustomer=null, string type=null)
        {

            var getItemUpdate = _pendingDataToSyncService.GetActivePendingDataToSync();
           

            if (identifier == ImporterIdentifier.CustomerImporter)
            {
                ImportCustomers((int)identifier, idcustomer, type);
            }

            if (identifier == ImporterIdentifier.ProductImporter)
            {
                ImportProducts((int)identifier, idcustomer, type);
            }

            if (identifier == ImporterIdentifier.OrderImporter)
            {
                ImportOrders((int)identifier, idcustomer, type);
            }

            if (identifier == ImporterIdentifier.PendingOrderImporter)
            {
                ImportPendingOrders();
            }

            if (identifier == ImporterIdentifier.ImageImporter)
            {
                ImportImages();
            }

            if (identifier == ImporterIdentifier.ImportDataFromNetsuite)
            {
                var getItemUpdateProduct = getItemUpdate.Where(r => r.Type == 1).ToList();

                var getItemUpdateCustomer = getItemUpdate.Where(r => r.Type == 2).ToList();

                var getItemUpdateOrder = getItemUpdate.Where(r => r.Type == 3).ToList();

                // var getItemInvoices = getItemUpdate.Where(r => r.Type == (int)ImporterIdentifierType.InvoicesSync).ToList();

                var getItemContacts = getItemUpdate.Where(r => r.Type == (int)ImporterIdentifierType.ContactCustomer).ToList();

                var getOrderPendingToSend = getItemUpdate.Where(r => r.Type == (int)ImporterIdentifierType.OrderSendNetsuite).ToList();

                var getInvoices = getItemUpdate.Where(r => r.Type == (int)ImporterIdentifierType.InvoicesSync).ToList();


                // Get and send orders 
                if (getOrderPendingToSend.Count() > 0 || identifier == ImporterIdentifier.OrderSendNetsuite)
                {

                    if (getOrderPendingToSend.Count() > 0)
                        ImportPendingOrders("SendOrderToNetsuite", getOrderPendingToSend);
                }

                // Get Invoices by customer
                if (getInvoices.Count() > 0)
                {
                    if (getInvoices.Count() > 0)
                        ImportOrders((int)identifier, idcustomer, "GetInvoiceByCustomer", getInvoices);
                }

                if (getItemUpdateProduct.Count() > 0)
                    ImportProducts((int)identifier, idcustomer, "GetDataFromNetsuite", getItemUpdateProduct);

                if (getItemUpdateCustomer.Count()>0)
                    ImportCustomers((int)identifier, idcustomer, "GetDataFromNetsuite", getItemUpdateCustomer);

                  if (getItemUpdateOrder.Count() > 0)
                    ImportOrders((int)identifier, idcustomer, "GetDataFromNetsuite", getItemUpdateOrder);

                //if (getItemInvoices.Count() > 0)
                //    ImportOrders((int)identifier, idcustomer, "SendInvoicesDataToNetsuite", getItemInvoices);

                if (getItemContacts.Count() > 0 || identifier == ImporterIdentifier.ContactCustomer)
                {
                    var getContactToUpdate = getItemUpdate.Where(r => r.Type == (int)ImporterIdentifier.ContactCustomer).ToList();

                    if (getContactToUpdate.Count() > 0)
                        ImportCustomers((int)identifier, idcustomer, "UpdateContact", getContactToUpdate);
                }

                
            }

            if (identifier == ImporterIdentifier.ContactCustomer)
            {
                var getContactToUpdate = getItemUpdate.Where(r => r.Type ==(int) ImporterIdentifier.ContactCustomer).ToList();
                
                if (getContactToUpdate.Count() > 0)
                    ImportCustomers((int)identifier, idcustomer, "UpdateContact", getContactToUpdate);
            }

            return _repositoryNetSuiteImporter.GetById((int)identifier);
        }

        public List<NetSuiteImporter> GetImporters()
        {
            return _repositoryNetSuiteImporter.Table.ToList();
        }

        public void UpdateImporter(NetSuiteImporter importer)
        {
            if (importer == null)
                throw new ArgumentException("importer");

            _repositoryNetSuiteImporter.Update(importer);
        }

        public void CreateImporter(NetSuiteImporter importer)
        {
            if (importer == null)
                throw new ArgumentException("importer");

            _repositoryNetSuiteImporter.Insert(importer);
        }

        public bool ImportCustomers(int identifier, string idcustomer = null, string type = null)
        {
            var dateInfo = _repositoryNetSuiteImporter.GetById((int)identifier);
            var date = "";
            if (dateInfo.LastExecutionDate != null)
            {
                var data = Convert.ToDateTime(dateInfo.LastExecutionDate).AddDays(-3);
                date = data.ToString("MM/dd/yyyy");
            }
            _customerService.ImportCustomers(date, idcustomer, type);

            return true;
        }

        public bool ImportCustomers(int identifier, string idcustomer = null, string type = null, List<PendingDataToSync> listcustomers =null)
        {
            var dateInfo = _repositoryNetSuiteImporter.GetById((int)identifier);
            var date = "";
            if (dateInfo.LastExecutionDate != null)
            {
               var data = Convert.ToDateTime(dateInfo.LastExecutionDate).AddDays(-3);
                date = data.ToString("MM/dd/yyyy");
            }
            _customerService.ImportCustomers(date, idcustomer, type, listcustomers);

            return true;
        }

        public bool ImportOrders(int identifier, string idorder = null, string type = null)
        {
            var dateInfo = _repositoryNetSuiteImporter.GetById((int)ImporterIdentifier.OrderImporter);
            var date = "";
            if (dateInfo.LastExecutionDate != null)
            {
                var data = Convert.ToDateTime(dateInfo.LastExecutionDate).AddDays(-2);
                date = data.ToString("MM/dd/yyyy");
            }

            _orderService.ImportOrders(date, idorder, type);

            return true;
        }

        public bool ImportOrders(int identifier, string idorder = null, string type = null, List<PendingDataToSync> listcustomers = null)
        {
            var dateInfo = _repositoryNetSuiteImporter.GetById((int)ImporterIdentifier.OrderImporter);
            var date = "";
            if (dateInfo.LastExecutionDate != null)
            {
                var data = Convert.ToDateTime(dateInfo.LastExecutionDate).AddDays(-2);
                date = data.ToString("MM/dd/yyyy");
            }

            _orderService.ImportOrders(date, idorder, type, listcustomers);

            return true;
        }


        public bool ImportPendingOrders(string type = null, List<PendingDataToSync> listCustomers = null)
        {
            var dateInfo = _repositoryNetSuiteImporter.GetById((int)ImporterIdentifier.ImportDataFromNetsuite);

            if (type == "SendOrderToNetsuite") {
                foreach (var item in listCustomers)
                {
                    try
                    {
                        _logger.Warning("Start send  order to netsuite: " + item.IdItem);

                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = true;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _orderService.CreateOrderInNetsuite(dateInfo?.LastExecutionDate.ToString(), item.IdItem);

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
            else
            {

                if (dateInfo.LastExecutionDate != null)
                {
                    _orderService.SyncPendingOrders(dateInfo.LastExecutionDate.ToString());
                }
                else
                {
                    _orderService.SyncPendingOrders();
                }
            }
            return true;
        }

        public bool ImportProducts(int identifier, string idproduct = null, string type = null)
        {
            var dateInfo = _repositoryNetSuiteImporter.GetById((int)ImporterIdentifier.ProductImporter);
            var date = "";
            if (dateInfo.LastExecutionDate != null)
            {
                var data = Convert.ToDateTime(dateInfo.LastExecutionDate).AddDays(-2);
                date = data.ToString("MM/dd/yyyy");
            }

            _productService.ImportProducts(date, idproduct, type);

            return true;
        }

        public bool ImportProducts(int identifier, string idproduct = null, string type = null, List<PendingDataToSync> listcustomers = null)
        {
            var dateInfo = _repositoryNetSuiteImporter.GetById((int)ImporterIdentifier.ProductImporter);
            var date = "";
            if (dateInfo.LastExecutionDate != null)
            {
                var data = Convert.ToDateTime(dateInfo.LastExecutionDate).AddDays(-2);
                date = data.ToString("MM/dd/yyyy");
            }

            _productService.ImportProducts(date, idproduct, type, listcustomers);

            return true;
        }

        public bool ImportImages()
        {
            // call jobs
            _iS3Service.ImportImages();

            return true;
        }

        

        #endregion
    }
}
