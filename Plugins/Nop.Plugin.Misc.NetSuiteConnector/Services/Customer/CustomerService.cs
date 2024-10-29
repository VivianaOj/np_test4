using Newtonsoft.Json;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.NN;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Address;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Custom;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.NN;
using Nop.Services.Security;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public partial class CustomerService : ICustomerService
    {
        #region Fields

        private IConnectionServices _connectionService;
        private ISettingService _settingService;
        private IAclService _aclService;

        private IRepository<CustomerCustomerRoleMapping> _customerCustomerRoleMappingRepository;

        private IRepository<Customer> _repository;
        private IRepository<Product> _productRespository;

        private  IAddressService _addressService;
        private  Nop.Services.Customers.ICustomerService _customerService;
        private  Nop.Services.Customers.ICompanyService _companyService;
         
        private  IStateProvinceService _stateProvinceService;
        private  ICountryService _countryService;
      
        private ILogger _logger;
        private INotificationService _notificationService;
        private  ILogImportNetsuiteService _logImportNetsuite;
        
        private IPendingDataToSyncService _pendingDataToSyncService;
        private IGenericAttributeService _genericAttributeService;

        private IProductPricesCustomerService _productPricesCustomerService;
        private IRepository<PriceLevel> _priceLevel;
        private IPriceLevelService _priceLevelService;
        private IItemPricingService _itemPricingService;
        private IItemCollectionServices _itemCollection;

        private const int RegisterRole = 3;
        private const int AccountCustomerRole = 6;
        Hashtable hashCustomer = new Hashtable();
        private string activeimportcustomers;
        string activeImport = "";
        private DateTime LastExecutionDateGeneral;

        #endregion

        #region Ctor

        public CustomerService(IRepository<Customer> repository, IConnectionServices connectionService,
            IAddressService addressService, Nop.Services.Customers.ICustomerService customerService,
            IProductPricesCustomerService productPricesCustomerService, Nop.Services.Customers.ICompanyService companyService,
            INotificationService notificationService, IRepository<CustomerCustomerRoleMapping> customerCustomerRoleMappingRepository,
            IStateProvinceService stateProvinceService, ICountryService countryService, IGenericAttributeService genericAttributeService, IItemCollectionServices itemCollection, ILogger logger, IAclService aclService,
            IPriceLevelService priceLevelService,
            IItemPricingService itemPricingService,
            IRepository<Product> productRespository, ISettingService settingService, ILogImportNetsuiteService logImportNetsuite, IRepository<PriceLevel> priceLevel,
            IPendingDataToSyncService pendingDataToSyncService)
        {
            _productRespository = productRespository;
            _itemPricingService = itemPricingService;
            _priceLevelService = priceLevelService;
            _repository = repository;
            _connectionService = connectionService;
            _addressService = addressService;
            _customerService = customerService;
            _productPricesCustomerService = productPricesCustomerService;
            _companyService = companyService;
            _notificationService = notificationService;
            _customerCustomerRoleMappingRepository = customerCustomerRoleMappingRepository;
            _stateProvinceService = stateProvinceService;
            _countryService = countryService;
            _genericAttributeService = genericAttributeService;
            _itemCollection = itemCollection;
            _logger = logger;
            _aclService = aclService;
            _settingService = settingService;
            _logImportNetsuite = logImportNetsuite;
            _priceLevel = priceLevel;
            _pendingDataToSyncService = pendingDataToSyncService;
        }

        #endregion
       
		#region Impor data from netsuite
	
		public void ImportCustomers(string LastExecutionDate, string idcustomer = null, string type = null)
        {
            activeimportcustomers = _settingService.GetSetting("netsuiteimportmodel.activeimportcustomers").Value;
            var charArray = activeimportcustomers.Split(",");
            LastExecutionDateGeneral = DateTime.Now;

            int i = 1;
            foreach (var item in charArray)
            {
                var id = item.Trim();

                if (i == 1)
                {
                    activeImport = "c.entitystatus =" + id;
                }
                   
                else
                {
                    if (i != charArray.Length)
                    {
                        activeImport = activeImport +" or c.entitystatus =" + id;
                    }
                    else
                    {
                        activeImport = activeImport +" or c.entitystatus =" + id;
                    }
                }
                i++;

            }
                if (type == "All")
            {
                UpdateCreateCustomer();
            }
            else if (type == "LastUpdate")
            {
                UpdateCreateCustomer(LastExecutionDate);
            }
            else if (type == "SpecificCustomerId")
            {
                GetSpecificCustomer(idcustomer);
            }
            else if (type == "GetDataFromNetsuite")
            {
                
                var getItemUpdateCustomer = _pendingDataToSyncService.GetActivePendingDataToSync().Where(r=>r.Type==2);
				foreach (var item in getItemUpdateCustomer)
				{
					try
					{
                        _logger.Warning("Start update Customer: " + item.IdItem);

                        GetSpecificCustomer(item.IdItem.ToString());

                        item.SuccessDate = DateTime.Now;
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = true;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Finished update Customer: " + item.IdItem);
                    }
					catch (Exception ex)
					{
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = false;
                        item.FailedDate = DateTime.Now;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Error update Customer: " + item.IdItem, ex);
                    }
                    
                }
            }
            else if (string.IsNullOrEmpty(type))
            {
                UpdateCreateCustomer(LastExecutionDate);
            }

        }

        public void ImportCustomers(string LastExecutionDate, string idcustomer = null, string type = null, List<PendingDataToSync> listCustomers = null)
        {
            activeimportcustomers = _settingService.GetSetting("netsuiteimportmodel.activeimportcustomers").Value;
            var charArray = activeimportcustomers.Split(",");
            LastExecutionDateGeneral = DateTime.Now;

            int i = 1;
            foreach (var item in charArray)
            {
                var id = item.Trim();

                if (i == 1)
                {
                    activeImport = "c.entitystatus =" + id;
                }

                else
                {
                    if (i != charArray.Length)
                    {
                        activeImport = activeImport + " or c.entitystatus =" + id;
                    }
                    else
                    {
                        activeImport = activeImport + " or c.entitystatus =" + id;
                    }
                }
                i++;

            }
            if (type == "All")
            {
                UpdateCreateCustomer();
            }
            else if (type == "LastUpdate")
            {
                UpdateCreateCustomer(LastExecutionDate);
            }
            else if (type == "SpecificCustomerId")
            {
                GetSpecificCustomer(idcustomer);
            }
            else if (type == "GetDataFromNetsuite")
            {

                //var getItemUpdateCustomer = _pendingDataToSyncService.GetActivePendingDataToSync().Where(r => r.Type == 2);
                foreach (var item in listCustomers)
                {
                    try
                    {
                        _logger.Warning("Start update Customer: " + item.IdItem);

                        GetSpecificCustomer(item.IdItem.ToString());

                        item.SuccessDate = DateTime.Now;
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = true;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Finished update Customer: " + item.IdItem);
                    }
                    catch (Exception ex)
                    {
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = false;
                        item.FailedDate = DateTime.Now;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Error update Customer: " + item.IdItem, ex);
                    }

                }
            }
            else if (type == "UpdateContact")
            {
                foreach (var item in listCustomers)
                {
                    try
                    {
                        _logger.Warning("Start UpdateContact: " + item.IdItem);

                        ContactUpdateNetsuite contactUpdateNetsuite = new ContactUpdateNetsuite();
                        contactUpdateNetsuite.custentity_registered_online = true; 

                        UpdateContactNetsuite(contactUpdateNetsuite, item.IdItem);

                        item.SuccessDate = DateTime.Now;
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = true;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Finished UpdateContact: " + item.IdItem);
                    }
                    catch (Exception ex)
                    {
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = false;
                        item.FailedDate = DateTime.Now;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Error update Customer: " + item.IdItem, ex);
                    }

                }
            }
            else if (string.IsNullOrEmpty(type))
            {
                UpdateCreateCustomer(LastExecutionDate);
            }

        }

        public async Task<bool> ImportCustomersAsync(Microsoft.EntityFrameworkCore.DbContext dbContext, string LastExecutionDate, string idcustomer = null, string type = null)
        {
			//using (var dbContextNop = _dbContext.CreateDbContext())
			//{
			//    activeimportcustomers = _settingService.GetSetting("netsuiteimportmodel.activeimportcustomers").Value;
			//}


			var _settingService = EngineContext.Current.Resolve<ISettingService>();
			activeimportcustomers = _settingService.GetSetting("netsuiteimportmodel.activeimportcustomers").Value;
			var charArray = activeimportcustomers.Split(",");
			LastExecutionDateGeneral = DateTime.Now;

			int i = 1;
			foreach (var item in charArray)
			{
				var id = item.Trim();

				if (i == 1)
				{
					activeImport = "c.entitystatus =" + id;
				}

				else
				{
					if (i != charArray.Length)
					{
						activeImport = activeImport + " or c.entitystatus =" + id;
					}
					else
					{
						activeImport = activeImport + " or c.entitystatus =" + id;
					}
				}
				i++;

			}
			if (type == "All")
            {
                UpdateCreateCustomer();
            }
            else if (type == "LastUpdate")
            {
                UpdateCreateCustomer(LastExecutionDate);
            }
            else if (type == "SpecificCustomerId")
            {
                await GetSpecificCustomerAsync(idcustomer).ConfigureAwait(false);
            }
            else if (string.IsNullOrEmpty(type))
            {
                UpdateCreateCustomer(LastExecutionDate);
            }
            return true;
        }
     
     
        private async Task GetSpecificCustomerAsync(string idcustomer)
        {
            char delimitador = '/';
            //_companyService = companyService;
            var Id = Convert.ToInt32(idcustomer);
            try
            {
                if (idcustomer != null)
                {
                    //Get Customer item from Netsuite 
                   var _companyService = EngineContext.Current.Resolve<Nop.Services.Customers.ICompanyService>();

                    //Get Customer item from Nopcommerce 
                    var ExistCustomer = _companyService.GetCompanyByNetSuiteId(Id);

                    var customerDTO = await GetCustomerAsync(Id);


                    if (ExistCustomer.Count == 0)
                    {
                        if (customerDTO?.EntityStatus != null)
                        {
                            //Validate only customer, pending add EntityStatus
                            if (activeimportcustomers.Contains(customerDTO.EntityStatus.Id))
                            {
                                // Create Company
                                Company company = CreateCompanyInfo(customerDTO);

                                // Create Relationship between companyand address
                                InsertCompanyAddress(delimitador, Id, customerDTO, company);

                                //Get ContactRole List from netsuite
                                var ListContactRoles =  GetContactsRole(Id);

                                if (ListContactRoles.TotalResults > 0)
                                {
                                    //Get ContactRole item from netsuite
                                    var customRole = _customerService.GetCustomerRoleById(AccountCustomerRole);

                                    foreach (var contactList in ListContactRoles.Items)
                                    {
                                        foreach (var contact in contactList.Links)
                                        {
                                            string[] IdContactList = contact.Href.Split(delimitador);
                                            var ContactId = Convert.ToInt32(IdContactList[IdContactList.Length - 1]);

                                            //Get ContactRole item from netsuite
                                            var contactRoleDto =  GetContact(ContactId);
                                            //Create customer
                                            int customerId = 0;
                                            if (contactRoleDto != null)
                                            {
                                                if (contactRoleDto.Email != null)
                                                {
                                                    Customer insertCustomer = InsertCustomer(Id, ContactId, contactRoleDto, customerDTO, company.Id);

                                                    customerId = insertCustomer.Id;
                                                    var address = _companyService.GetAllCompanyAddressMappingsById(company.Id);
                                                    UpdateCustomerAddress(insertCustomer, address, company, contactRoleDto);
                                                    InsertCompanyCustomer(company, insertCustomer);
                                                    CustomerRoleAccount(customRole, insertCustomer);
                                                }

                                            }

                                            ItemCollection(customerDTO, company, customerId);
                                        }
                                    }
                                }

                                //valid active contact
                                InactiveContactByCompany(company.Id);
                            }
                        }
                    }
                    else
                    {
                        var updateRole = false;
                        if (updateRole)
                            CustomerRoleAccountUpdateList();

                        UpdateCompany(customerDTO, ExistCustomer, Id);

                        foreach (var comp in ExistCustomer)
                        {
                            //Get ContactRole List from netsuite
                            var ListContactRoles = GetContactsRole(Id);

                            if (ListContactRoles.TotalResults > 0)
                            {
                                _customerService = EngineContext.Current.Resolve<Nop.Services.Customers.ICustomerService>();
                                //Get ContactRole item from netsuite
                                var customRole = _customerService.GetCustomerRoleById(AccountCustomerRole);

                                // var _customerService = EngineContext.Current.Resolve<Nop.Services.Customers.ICustomerService>();
                                _companyService = EngineContext.Current.Resolve<Nop.Services.Customers.ICompanyService>();

                                //Disable All customer address
                                var updateAddress = _companyService.GetAllCompanyAddressMappingsList(null, comp.Id);

                                foreach (var item in updateAddress)
                                {
                                    item.Address.Active = true;
                                    _companyService.UpdateCompanyAddressMapping(item);
                                }

                                foreach (var contactList in ListContactRoles.Items)
                                {
                                    foreach (var contact in contactList.Links)
                                    {
                                        string[] IdContactList = contact.Href.Split(delimitador);
                                        var ContactId = Convert.ToInt32(IdContactList[IdContactList.Length - 1]);

                                        //Get ContactRole item from netsuite
                                        var contactRoleDto =  GetContact(ContactId);
                                        //Create customer
                                        int customerId = 0;
                                        if (contactRoleDto != null)
                                        {
                                            if (contactRoleDto.Email != null)
                                            {
                                                Customer insertCustomer = InsertCustomer(Id, ContactId, contactRoleDto, customerDTO, comp.Id);

                                                customerId = insertCustomer.Id;
                                                UpdateAddressCompany(Id, customerDTO, comp.Id, comp, insertCustomer);
                                                var addresses = _companyService.GetAllCompanyAddressMappingsById(comp.Id);

                                                UpdateCustomerAddress(insertCustomer, addresses, comp, contactRoleDto);


                                                var companyUpdate = _companyService.GetAllCompanyCustomerMappings(insertCustomer.Id, comp.Id);
                                                if (companyUpdate.Count == 0)
                                                    InsertCompanyCustomer(comp, insertCustomer);

                                                UpdateContactRole(customerDTO, ExistCustomer, Id, Id, comp);
                                            }
                                        }


                                        ItemCollection(customerDTO, comp, customerId);
                                    }
                                }
                            }

                            //valid active contact
                            InactiveContactByCompany(comp.Id);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Error("Import customer GetSpecificCustomerAsync", ex);

                _notificationService.ErrorNotification(ex);
                throw ex.InnerException;
            }

        }
		
		private async Task<CustomerDto> GetCustomerAsync(int Id)
		{
			//Assuming _connectionService.GetConnectionAsync is available and returns Task < string >
			var getData = await _connectionService.GetConnectionAsync("/record/v1/customer/" + Id, "GET");

			//Assuming CustomerDto is the deserialization target
			var customerDto = JsonConvert.DeserializeObject<CustomerDto>(getData);

			return customerDto;
		}

	
		private void UpdateCreateCustomer(string LastExecutionDate)
        {
            char delimitador = '/';
            string dateLimit = "";
            LastExecutionDate = DateTime.Now.AddDays(-3).ToString();
            int TotalResults = 1;

            for (int i = 0; i < TotalResults; i = i + 1000)
            {
                try
                {
                    if (i != 0)
                        dateLimit = "?limit=1000&offset=" + i;
                    // dateLimit = "?limit=1000&offset=" + i;
                    //Get Customer List from Netsuite 
                    var ListCustomer = GetCustomers(dateLimit, LastExecutionDate, activeImport );
                    if (ListCustomer != null)
                    {
                        TotalResults = ListCustomer.TotalResults;
                        UpdateCreateCustomerCustomer(ListCustomer);
                    }
                    //dateLimit = "?limit=1000&offset=" + i;
                }
                catch (Exception)
                {

                }
               
            }
        }

        private void UpdateCreateCustomer()
        {
            char delimitador = '/';
            string dateLimit = "";
            int TotalResults = 1;

            for (int i = 0; i < TotalResults; i = i + 1000)
            {
                try
                {
                    if(i!=0)
                        dateLimit = "?limit=1000&offset=" + i;
                    // dateLimit = "?limit=1000&offset=" + i;
                    //Get Customer List from Netsuite 
                    var ListCustomer = GetCustomers(dateLimit, activeImport);
                    if (ListCustomer != null)
                    {
                        TotalResults = ListCustomer.TotalResults;
                        UpdateCreateCustomerCustomer(ListCustomer);
                    }
                   
                }
                catch (Exception)
                {

                }
            }
        }

        private  void GetSpecificCustomer(string idcustomer)
        {
            char delimitador = '/';

            var Id = Convert.ToInt32(idcustomer);

            if (idcustomer != null)
            {
                //Get Customer item from Netsuite 
                var customerDTO = GetCustomer(Id);

                //Get Customer item from Nopcommerce 
                var ExistCustomer = _companyService.GetCompanyByNetSuiteId(Id);

                if (ExistCustomer.Count == 0)
                {
                    if (customerDTO?.EntityStatus != null)
                    {
                        //Validate only customer, pending add EntityStatus
                        if (activeimportcustomers.Contains(customerDTO.EntityStatus.Id))
                        {
                            // Create Company
                            Company company = CreateCompanyInfo(customerDTO);

                            // Create Relationship between companyand address
                            InsertCompanyAddress(delimitador, Id, customerDTO, company);

                            //Get ContactRole List from netsuite
                            var ListContactRoles = GetContactsRole(Id);

                            if (ListContactRoles.TotalResults > 0)
                            {
                                //Get ContactRole item from netsuite
                                var customRole = _customerService.GetCustomerRoleById(AccountCustomerRole);

                                foreach (var contactList in ListContactRoles.Items)
                                {
                                    foreach (var contact in contactList.Links)
                                    {
                                        string[] IdContactList = contact.Href.Split(delimitador);
                                        var ContactId = Convert.ToInt32(IdContactList[IdContactList.Length - 1]);

                                        //Get ContactRole item from netsuite
                                        var contactRoleDto = GetContact(ContactId);
                                        //Create customer
                                        int customerId = 0;
                                        if (contactRoleDto != null)
                                        {
                                            if (contactRoleDto.Email != null)
                                            {
                                                Customer insertCustomer = InsertCustomer(Id, ContactId, contactRoleDto, customerDTO, company.Id);

                                                customerId = insertCustomer.Id;
                                                var address = _companyService.GetAllCompanyAddressMappingsById(company.Id);
                                                UpdateCustomerAddress(insertCustomer, address, company, contactRoleDto);
                                                InsertCompanyCustomer(company, insertCustomer);
                                                CustomerRoleAccount(customRole, insertCustomer);
                                            }

                                        }

                                        ItemCollection(customerDTO, company, customerId);
                                    }
                                }
                            }

                            //valid active contact
                            InactiveContactByCompany(company.Id);
                        }
                    }
                }
                else
                {
                    var updateRole = false;
                    if (updateRole)
                        CustomerRoleAccountUpdateList();

                    UpdateCompany(customerDTO, ExistCustomer, Id);

                    foreach (var comp in ExistCustomer)
                    {
                        //Get ContactRole List from netsuite
                        var ListContactRoles = GetContactsRole(Id);

                        if (ListContactRoles.TotalResults > 0)
                        {
                            //Get ContactRole item from netsuite
                            var customRole = _customerService.GetCustomerRoleById(AccountCustomerRole);

                            //Disable All customer address
                            var updateAddress = _companyService.GetAllCompanyAddressMappingsList(null, comp.Id);

                            foreach (var item in updateAddress)
                            {
                                item.Address.Active = true;
                                _companyService.UpdateCompanyAddressMapping(item);
                            }

                            foreach (var contactList in ListContactRoles.Items)
                            {
                                foreach (var contact in contactList.Links)
                                {
                                    string[] IdContactList = contact.Href.Split(delimitador);
                                    var ContactId = Convert.ToInt32(IdContactList[IdContactList.Length - 1]);

                                    //Get ContactRole item from netsuite
                                    var contactRoleDto = GetContact(ContactId);
                                    //Create customer
                                    int customerId = 0;
                                    if (contactRoleDto != null)
                                    {
                                        if (contactRoleDto.Email != null)
                                        {
                                            Customer insertCustomer = InsertCustomer(Id, ContactId, contactRoleDto, customerDTO, comp.Id);

                                            customerId = insertCustomer.Id;
                                            UpdateAddressCompany(Id, customerDTO, comp.Id, comp, insertCustomer);
                                            var addresses = _companyService.GetAllCompanyAddressMappingsById(comp.Id);

                                            UpdateCustomerAddress(insertCustomer, addresses, comp, contactRoleDto);


                                            var companyUpdate = _companyService.GetAllCompanyCustomerMappings(insertCustomer.Id, comp.Id);
                                            if (companyUpdate.Count == 0)
                                                InsertCompanyCustomer(comp, insertCustomer);

                                            UpdateContactRole(customerDTO, ExistCustomer, Id, Id, comp);
                                        }
                                    }


                                    ItemCollection(customerDTO, comp, customerId);
                                }
                            }
                        }

                        //valid active contact
                        InactiveContactByCompany(comp.Id);
                    }
                }
            }
        }

        private void InactiveContactByCompany(int companyId)
        {
            var companyCustomer = _companyService.GetAllCompanyCustomerMappings(null, companyId);
            foreach (var item in companyCustomer)
            {
                try
                {
                    if (!hashCustomer.ContainsValue(item.CustomerId))
                    {
                        item.Active = false;
                        _companyService.UpdateCompanyCustomerMapping(item);
                    }
                }
                catch (Exception ex)
                {
                    _logger = EngineContext.Current.Resolve<ILogger>();
                    _logger = EngineContext.Current.Resolve<ILogger>(); _logger.Warning("ImportCustomerError:: InactiveContactByCompany:: companyId" + companyId + " CustomerItem item: " + item.Id + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                }
            }
        }
        #endregion  

        #region Customer Info
        private void UpdateCreateCustomerCustomer(CustomersObject ListCustomer)
        {
            char delimitador = '/';


            if (ListCustomer.Count > 0)
            {
                int i = 0;
                foreach (var item in ListCustomer.Items)
                {
                    try
                    {
                        //Get Customer item from Nopcommerce 
                        var ExistCustomer = _companyService.GetCompanyByNetSuiteId(item.Id);
                        //if (ExistCustomer.Count == 0)
                        //{
                        //Get Customer item from Netsuite 
                        var customerDTO = GetCustomer(item.Id);

                        if (ExistCustomer.Count == 0)
                        {
                           // var customerDTO = GetCustomer(item.Id);
                            if (customerDTO?.EntityStatus != null)
                            {
                                //Validate only customer, pending add EntityStatus
                                if (activeimportcustomers.Contains(customerDTO.EntityStatus.Id))
                                {
                                    // Create Company
                                    Company company = CreateCompanyInfo(customerDTO);

                                    // Create Relationship between companyand address
                                    InsertCompanyAddress(delimitador, item.Id, customerDTO, company);

                                    //Get ContactRole List from netsuite
                                    var ListContactRoles = GetContactsRole(item.Id);

                                    if (ListContactRoles.TotalResults > 0)
                                    {
                                        //Get ContactRole item from netsuite
                                        var customRole = _customerService.GetCustomerRoleById(AccountCustomerRole);

                                        foreach (var contactList in ListContactRoles.Items)
                                        {
                                            foreach (var contact in contactList.Links)
                                            {
                                                string[] IdContactList = contact.Href.Split(delimitador);
                                                var ContactId = Convert.ToInt32(IdContactList[IdContactList.Length - 1]);

                                                //Get ContactRole item from netsuite
                                                var contactRoleDto = GetContact(ContactId);
                                                //Create customer
                                                int customerId = 0;
                                                if (contactRoleDto != null)
                                                {
                                                    if (contactRoleDto.Email != null)
                                                    {
                                                        Customer insertCustomer = InsertCustomer(item.Id, ContactId, contactRoleDto, customerDTO, company.Id);

                                                        customerId = insertCustomer.Id;
                                                        var address = _companyService.GetAllCompanyAddressMappingsById(company.Id);
                                                        UpdateCustomerAddress(insertCustomer, address, company, contactRoleDto);
                                                        InsertCompanyCustomer(company, insertCustomer);
                                                        CustomerRoleAccount(customRole, insertCustomer);
                                                    }
                                                }
                                                ItemCollection(customerDTO, company, customerId);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            if (activeimportcustomers.Contains(customerDTO.EntityStatus.Id))
                            {
                                
                                 var updateRole = false;
                                 if (updateRole)
                                 {
                                     CustomerRoleAccountUpdateList();
                                 }

                                 UpdateCompany(customerDTO, ExistCustomer, item.Id);
                                 foreach (var comp in ExistCustomer)
                                 {

                                     UpdateAddressCompany(item.Id, customerDTO, comp.Id, comp, null);

                                     // Create Relationship between companyand address
                                     UpdateContactRole(customerDTO, ExistCustomer, item.Id, item.Id, comp);

                                     ItemCollection(customerDTO, comp, item.Id);
                                 }
                            }
                        }

                        i++;
                    }
                    catch (Exception ex)
                    {
                         _logger = EngineContext.Current.Resolve<ILogger>();
                        _logger.Warning("ImportCustomerError:: UpdateCreateCustomerCustomer:: companyId" + item.Id + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                    }
                }
            }
        }

        private void ItemCollection(CustomerDto customerDTO, Company company, int customerId)
        {
            try
            {
                //create Item Collection
                if (customerDTO.ItemCollection != null && customerId!=0)
                {
                    var collection = _itemCollection.GetItemCollectionById(Convert.ToInt32(customerDTO.ItemCollection.Id));
                    if (collection == null)
                    {
                        _logger.Warning("Insert collection+" + customerDTO.ItemCollection);
                        ItemCollection itemCollection = new ItemCollection();
                        itemCollection.NetsuiteId = Convert.ToInt32(customerDTO.ItemCollection.Id);
                        itemCollection.Name = customerDTO.ItemCollection.RefName;

                        _itemCollection.InsertItemCollection(itemCollection);

                        //create product item collection
                        ItemCollectionCompany itemCollectionCompany = new ItemCollectionCompany();
                        itemCollectionCompany.CollectionId = itemCollection.Id;
                        itemCollectionCompany.CustomerId = customerId;
                        itemCollectionCompany.CustomerNetsuiteId = Convert.ToInt32(company.Id);
                        _itemCollection.InsertItemCollectionCompany(itemCollectionCompany);



                    }
                    else
                    {

                        if (customerId != 0)
                        {
                            var collectionCompany = _itemCollection.GetItemCollectionCompanyById(customerId);
                            if (collectionCompany.Count == 0)
                            {
                                //create product item collection
                                ItemCollectionCompany itemCollectionCompany = new ItemCollectionCompany();
                                itemCollectionCompany.CollectionId = Convert.ToInt32(collection.Id);
                                itemCollectionCompany.CustomerId = customerId;
                                itemCollectionCompany.CustomerNetsuiteId = Convert.ToInt32(company.Id);
                                _itemCollection.InsertItemCollectionCompany(itemCollectionCompany);
                            }
                            else
                            {
                                foreach (var item in collectionCompany)
                                {
                                    item.CollectionId = collection.Id;
                                    _itemCollection.UpdateIItemCollectionCompany(item);
                                }

                                var companyItem = _itemCollection.GetItemCollectionCompanyByNetsuiteId(Convert.ToInt32(company.Id));

                                foreach (var item in companyItem)
                                {
                                    item.CollectionId = collection.Id;
                                    _itemCollection.UpdateIItemCollectionCompany(item);
                                }
                            }
                        }
                        


                    }

                }
            }
            catch (Exception ex)
            {
                _notificationService = EngineContext.Current.Resolve<INotificationService>();
                _logger = EngineContext.Current.Resolve<ILogger>();

                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportCustomerError:: ItemCollection:: companyId" + company.Id + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

        }

        private void UpdateAddressCompany(int Id, CustomerDto customerDTO, int CompanyId, Company Company, Customer customer)
        {
            try
            {
                if (Id != null)
                {
                    _addressService = EngineContext.Current.Resolve<IAddressService>();
                    _companyService = EngineContext.Current.Resolve<Nop.Services.Customers.ICompanyService>();

                    var ListAdress = GetAddressBook(Id);
                    if (ListAdress != null)
                    {
                        // Create Address
                        foreach (var addressBook in ListAdress.Items)
                        {
                            foreach (var address in addressBook.Links)
                            {
                                string[] id = address.Href.Split('/');
                                var ultimoId = Convert.ToInt32(id[id.Length - 1]);

                                var AddressBookDTO = GetAddress(Id, ultimoId);

                                var GetAddressBook = GetAddressBookAddress(Id, ultimoId);
                                var country = new Country();
                                var state = new StateProvince();

                                if (GetAddressBook != null)
                                {
                                    _countryService = EngineContext.Current.Resolve<ICountryService>();
                                    _stateProvinceService = EngineContext.Current.Resolve<IStateProvinceService>();

                                    country = _countryService.GetCountryByTwoLetterIsoCode(GetAddressBook.Country?.Id);
                                    state = _stateProvinceService.GetStateProvinceByAbbreviation(GetAddressBook.State);
                                }

                                var AddressInfo = new List<Address>();
                                if (customer!=null)
                                    AddressInfo = _addressService.GetAddressByNetsuiteId(AddressBookDTO.Id, customer.Email);
                                else
                                    AddressInfo = _addressService.GetAddressByNetsuiteId(AddressBookDTO.Id);

                                if (AddressInfo.Count == 0)
                                {
                                    //Insert Address
                                    Address insertAddress = new Address();
                                    if (customer?.Email != null)
                                        insertAddress.Email = customer?.Email;
                                    else
                                    {
                                        if (Company.Email != null)
                                            insertAddress.Email = Company.Email;
                                        else
                                            insertAddress.Email = "info@yourstore.com";
                                    }

                                    insertAddress.FirstName = GetAddressBook.Addressee;
                                    insertAddress.LastName = GetAddressBook.City;
                                    insertAddress.Company = customerDTO?.CompanyName;
                                    insertAddress.Address1 = GetAddressBook.Addr1;
                                    insertAddress.Address2 = GetAddressBook.Addr2;
                                    insertAddress.CreatedOnUtc = DateTime.UtcNow;
                                    insertAddress.NetsuitId = AddressBookDTO.Id;

                                    insertAddress.Country = country;
                                    insertAddress.StateProvince = state;
                                    insertAddress.CountryId = country?.Id;
                                    insertAddress.StateProvinceId = state?.Id;
                                    insertAddress.City = GetAddressBook.City;
                                    insertAddress.ZipPostalCode = GetAddressBook.Zip;

                                    if (GetAddressBook.AddrPhone != null)
                                        insertAddress.PhoneNumber = GetAddressBook.AddrPhone;
                                    else
                                        insertAddress.PhoneNumber = "0";

                                    insertAddress.Active = false;
                                    _addressService.InsertAddress(insertAddress);

                                    if (CompanyId != 0)
                                    {
                                        CompanyAddresses CompanyAddress = new CompanyAddresses();
                                        CompanyAddress.CompanyId = CompanyId;
                                        CompanyAddress.AddressId = insertAddress.Id;

                                        CompanyAddress.IsBilling = AddressBookDTO.DefaultBilling;
                                        CompanyAddress.IsShipping = AddressBookDTO.DefaultShipping;
                                        CompanyAddress.Label = AddressBookDTO.Label;

                                        //if (customerDTO?.Deliveryroute != null)
                                        //{
                                            if (GetAddressBook.APPROVEDROUTEDELIVERY != null)
                                            {
                                                if (GetAddressBook.APPROVEDROUTEDELIVERY?.Id!=null)
                                                {
                                                    CompanyAddress.DeliveryRoute = true;
                                                    CompanyAddress.DeliveryRouteId = GetAddressBook?.APPROVEDROUTEDELIVERY?.Id;
                                                    CompanyAddress.DeliveryRouteName = GetAddressBook?.APPROVEDROUTEDELIVERY?.ReferenceName;
                                                }
                                                else
                                                    CompanyAddress.DeliveryRouteId = "0";
                                            }
                                            else
                                                CompanyAddress.DeliveryRouteId = "0";
                                        //}
                                        //else
                                        //    CompanyAddress.DeliveryRouteId = "0";

                                        _companyService.InsertCompanyAddressMapping(CompanyAddress);
                                    }
                                }
                                else
                                {
                                    foreach (var x in AddressInfo)
                                    {
                                        if (x.Email == null)
                                        {
                                            if (Company.Email != null)
                                                x.Email = Company.Email;
                                            else
                                                x.Email = "info@yourstore.com";
                                        }
                                        
                                        x.Company = customerDTO?.CompanyName;
                                        x.CountryId = country?.Id;
                                        x.StateProvinceId = state?.Id;
                                        x.Country = country;
                                        x.StateProvince = state;
                                        x.City = GetAddressBook.City;
                                        x.Address1 = GetAddressBook.Addr1;
                                        x.Address2 = GetAddressBook.Addr2;
                                        x.ZipPostalCode = GetAddressBook.Zip;
                                        x.Active = false;
                                        if (x.PhoneNumber == null)
                                        {
                                            if (GetAddressBook.AddrPhone != null)
                                                x.PhoneNumber = GetAddressBook.AddrPhone;
                                            else
                                                x.PhoneNumber = "0";
                                        }

                                        x.NetsuitId = ultimoId;

                                        _addressService.UpdateAddress(x);

                                        if (CompanyId != 0)
                                        {
                                            var companyAddres = _companyService.GetCompanyByAddressId(x.Id);
                                            if (companyAddres == null)
                                            {
                                                CompanyAddresses CompanyAddress = new CompanyAddresses();
                                                CompanyAddress.CompanyId = CompanyId;
                                                CompanyAddress.AddressId = x.Id;

                                                CompanyAddress.IsBilling = AddressBookDTO.DefaultBilling;
                                                CompanyAddress.IsShipping = AddressBookDTO.DefaultShipping;
                                                CompanyAddress.Label = AddressBookDTO.Label;
                                                //if (customerDTO?.Deliveryroute != null)
                                                //{
                                                    if (GetAddressBook.APPROVEDROUTEDELIVERY != null)
                                                    {
                                                        if (GetAddressBook.APPROVEDROUTEDELIVERY?.Id!=null)
                                                        {
                                                            CompanyAddress.DeliveryRoute = true;
                                                            CompanyAddress.DeliveryRouteId = GetAddressBook?.APPROVEDROUTEDELIVERY?.Id;
                                                            CompanyAddress.DeliveryRouteName = GetAddressBook?.APPROVEDROUTEDELIVERY?.ReferenceName;

                                                        }
                                                        else
                                                            CompanyAddress.DeliveryRouteId = "0";
                                                    }
                                                    else
                                                        CompanyAddress.DeliveryRouteId = "0";
                                                //}
                                                //else
                                                //    CompanyAddress.DeliveryRouteId = "0";

                                                _companyService.InsertCompanyAddressMapping(CompanyAddress);

                                            }
                                            else
                                            {
                                                companyAddres.IsBilling = AddressBookDTO.DefaultBilling;
                                                companyAddres.IsShipping = AddressBookDTO.DefaultShipping;
                                                companyAddres.Label = AddressBookDTO.Label;

                                                //if (customerDTO?.Deliveryroute != null)
                                                //{
                                                    if (GetAddressBook.APPROVEDROUTEDELIVERY != null)
                                                    {
                                                        if (GetAddressBook.APPROVEDROUTEDELIVERY?.Id!=null)
                                                        {
                                                            companyAddres.DeliveryRoute = true;
                                                            companyAddres.DeliveryRouteId = GetAddressBook?.APPROVEDROUTEDELIVERY?.Id;
                                                            companyAddres.DeliveryRouteName = GetAddressBook?.APPROVEDROUTEDELIVERY?.ReferenceName;

                                                        }
                                                        else
                                                            companyAddres.DeliveryRouteId = "0";

                                                    }
                                                    else
                                                        companyAddres.DeliveryRouteId = "0";
                                                //}
                                                //else
                                                //    companyAddres.DeliveryRouteId = "0";

                                                _companyService.UpdateCompanyAddressMapping(companyAddres);
                                            }
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
                 _logger = EngineContext.Current.Resolve<ILogger>();
                _logger.Warning("ImportCustomerError::  Update Address Company:: Company.NetsuiteId" + Company.NetsuiteId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }
       
        private void UpdateContactRole(CustomerDto customerDTO, List<Company> existCustomer, int idCustomer, int Id, Company companyId)
        {
            try
            {
                //Get ContactRole List from netsuite
                var ListContactRoles = GetContactsRole(idCustomer);

                if (companyId != null)
                {
                    var actualContact = _companyService.GetAllCompanyCustomerMappings(null, companyId.Id);

                    if (actualContact.Count > 0)
                    {
                        foreach (var item in actualContact)
                        {
                            var Customer = _customerService.GetCustomerById(item.CustomerId);
                            Customer.Active = false;

                            _customerService.UpdateCustomer(Customer);
                        }
                    }
                }

                if (ListContactRoles.TotalResults > 0)
                {
                    //Get ContactRole item from netsuite
                    var customRole = _customerService.GetCustomerRoleById(AccountCustomerRole);

                    foreach (var contactList in ListContactRoles.Items)
                    {
                        foreach (var contact in contactList.Links)
                        {
                            string[] IdContactList = contact.Href.Split('/');
                            var ContactId = Convert.ToInt32(IdContactList[IdContactList.Length - 1]);

                            //Get ContactRole item from netsuite
                            var contactRoleDto = GetContact(ContactId);
                            if (contactRoleDto != null)
                            {
                                var customer = _customerService.GetCustomerByNetsuitId(Convert.ToInt32(contactRoleDto.Id), contactRoleDto.Email );
                                if (customer != null)
                                {
                                    if (customer.Email != null)
                                    {
                                        UpdateCustomer(contactRoleDto, customer, companyId, customerDTO);
                                        CustomerRoleAccount(customRole, customer);
                                    }
                                }
                                else
                                {
                                    if (contactRoleDto.Email != null)
                                    {
                                        //Insert customer
                                        customer = InsertCustomer(Id, ContactId, contactRoleDto, customerDTO, companyId.Id);
                                        CustomerRoleAccount(customRole, customer);

                                    }
                                }
                                if (customer != null)
                                {
                                    var companyUpdate = _companyService.GetAllCompanyCustomerMappings(customer.Id, companyId.Id);
                                    if (companyUpdate.Count == 0)
                                        InsertCompanyCustomer(companyId, customer);
                                }

                            }


                        }
                    }
                }
            }
            catch (Exception ex)
            {
                 _logger = EngineContext.Current.Resolve<ILogger>();
                _logger.Warning("ImportCustomerError::  UpdateContactRole:: idCustomer " + idCustomer + " CompanyName " + companyId.CompanyName + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private void CustomerRoleAccount(CustomerRole customRole, Customer insertCustomer)
        {
            try
            {
                _customerService = EngineContext.Current.Resolve<Nop.Services.Customers.ICustomerService>();
                _customerCustomerRoleMappingRepository = EngineContext.Current.Resolve<IRepository<CustomerCustomerRoleMapping>>();

                var customerRole = _customerService.GetCustomerCustomerOrleById(insertCustomer.Id);
                var register = false;
                var accRole = false;

                if (customerRole.Count > 0)
                {
                    foreach (var item in customerRole)
                    {
                        if (item.CustomerRoleId == RegisterRole)
                            register = true;

                        if (item.CustomerRoleId == AccountCustomerRole)
                            accRole = true;
                    }

                }

                if (!register)
                {
                    CustomerCustomerRoleMapping CustomerRole = new CustomerCustomerRoleMapping();
                    CustomerRole.CustomerId = insertCustomer.Id;
                    CustomerRole.CustomerRoleId = RegisterRole;
                    _customerCustomerRoleMappingRepository.Insert(CustomerRole);
                }

                if (customRole != null)
                {
                    if (!accRole)
                    {
                        CustomerCustomerRoleMapping CustomerRoleAccount = new CustomerCustomerRoleMapping();
                        CustomerRoleAccount.CustomerId = insertCustomer.Id;
                        CustomerRoleAccount.CustomerRoleId = AccountCustomerRole;
                        _customerCustomerRoleMappingRepository.Insert(CustomerRoleAccount);
                    }
                }
            }
            catch (Exception ex)
            {
                 _logger = EngineContext.Current.Resolve<ILogger>();
                _logger.Warning("ImportCustomerError::  CustomerRoleAccount:: idCustomerNP " + insertCustomer.Id + " insertCustomerName " + insertCustomer.Email+ " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private void InsertCompanyCustomer(Company company, Customer insertCustomer)
        {
            try
            {
                var CompanyCustomer = _companyService.GetAllCompanyCustomerMappingsActiveInactive(insertCustomer.Id, company.Id);
                if (CompanyCustomer.Count == 0)
                {
                    CompanyCustomerMapping company_Customer = new CompanyCustomerMapping();
                    company_Customer.CompanyId = company.Id;
                    company_Customer.CustomerId = insertCustomer.Id;
                    company_Customer.Active = true;
                    _companyService.InsertCompanyCustomerMapping(company_Customer);
                }
                else
                {
                    foreach (var item in CompanyCustomer)
                    {
                        item.Active = true;
                        _companyService.UpdateCompanyCustomerMapping(item);
                    }
                }
            }
            catch (Exception ex)
            {
                 _logger = EngineContext.Current.Resolve<ILogger>();
                _logger.Warning("ImportCustomerError::  InsertCompanyCustomer:: idCustomerNP " + insertCustomer.Id + " insertCustomerName " + insertCustomer.Email + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private void UpdateCustomerAddress(Customer insertCustomer, IList<CompanyAddresses> address, Company company, ContactObject contactObject = null)
        {
            _customerService = EngineContext.Current.Resolve<Nop.Services.Customers.ICustomerService>();
            // var address = address.
            foreach (var x in address)
            {

                var addressId = _addressService.GetAddressById(x.AddressId);
                if (addressId != null)
                {
                    if (addressId.Email == insertCustomer.Email)
                    {
                        var Validation = false;
                        foreach (var item in insertCustomer.CustomerAddressMappings)
                        {
                            if (item.AddressId == x.AddressId && item.CustomerId == insertCustomer.Id)
                                Validation = true;
                        }
                        if (Validation == false)
                        {
                            CustomerAddressMapping customerAddress = new CustomerAddressMapping();
                            customerAddress.CustomerId = insertCustomer.Id;
                            customerAddress.AddressId = x.AddressId;
                            customerAddress.IsInherited = true;
                            insertCustomer.CustomerAddressMappings.Add(customerAddress);
                        }

                        if (addressId != null)
                        {
                            //var ListAdress = GetAddressBook(Convert.ToInt32(company.NetsuiteId));

                            // var AddressBookDTO = GetAddress(Convert.ToInt32(company.NetsuiteId), x.Address.NetsuitId);


                            Address add = _addressService.GetAddressById(addressId.Id);

                            if (add.FirstName == null)
                            {
                                if (contactObject == null)
                                {
                                    var name = _genericAttributeService.GetAttribute<string>(insertCustomer, NopCustomerDefaults.FirstNameAttribute);

                                    add.FirstName = name;
                                }
                                else
                                {
                                    if (contactObject.FirstName != null)
                                        add.FirstName = contactObject.FirstName;
                                }

                            }
                            else
                            {
                                if (add.FirstName != contactObject.FirstName)
                                {
                                    if (contactObject.FirstName != null)
                                        add.FirstName = contactObject.FirstName;
                                }
                            }

                            if (add.LastName == null)
                            {
                                if (contactObject == null)
                                {
                                    var name = _genericAttributeService.GetAttribute<string>(insertCustomer, NopCustomerDefaults.LastNameAttribute);

                                    add.LastName = name;
                                }
                                else
                                {
                                    if (contactObject.LastName != null)
                                        add.LastName = contactObject.LastName;
                                }

                            }
                            else
                            {
                                if (add.LastName != contactObject.LastName)
                                {
                                    if (contactObject.LastName != null)
                                        add.LastName = contactObject.LastName;
                                }
                            }

                            if (add.Email == null)
                            {
                                if (company.Email != null)
                                    add.Email = company.Email;
                                else
                                    add.Email = "info@yourstore.com";
                            }

                            if (add.PhoneNumber == null)
                            {
                                add.PhoneNumber = company.Phone;
                            }
                            else
                            {
                                if (add.PhoneNumber != contactObject.Phone)
                                {
                                    if (contactObject.Phone != null)
                                        add.PhoneNumber = contactObject.Phone;
                                }
                            }

                            if (addressId.NetsuitId != 0 && company.NetsuiteId != null)
                            {
                                var GetAddressBook = GetAddressBookAddress(Convert.ToInt32(company.NetsuiteId), addressId.NetsuitId);

                                if (GetAddressBook != null)
                                    add.Active = false;
                            }

                            if (add.Id > 0)
                                _addressService.UpdateAddress(add);
                            else
                                _addressService.InsertAddress(add);
                        }

                        if (x.IsBilling)
                        {
                            insertCustomer.BillingAddressId = addressId.Id;
                            insertCustomer.BillingAddress = addressId;
                        }
                        if (x.IsShipping)
                        {
                            insertCustomer.ShippingAddressId = addressId.Id;
                            insertCustomer.ShippingAddress = addressId;
                        }

                        _customerService.UpdateCustomer(insertCustomer);
                    }

                }
            }
        }

        private Customer InsertCustomer(int Id, int ContactId, ContactObject contactRoleDto, CustomerDto customerNetSuite, int companyId)
        {
            Customer insertCustomer = new Customer();
            Customer insertCustomerJson = new Customer();
            PriceLevel currentPriceLevel = null;
            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            try
            {
                _priceLevelService = EngineContext.Current.Resolve<IPriceLevelService>();


                if (!string.IsNullOrEmpty(customerNetSuite.PriceLevel?.Id))
                    currentPriceLevel = _priceLevelService.GetExistsPriceLevel(customerNetSuite.PriceLevel?.Id);

                if (!string.IsNullOrEmpty(contactRoleDto.Id))
                {
                    _customerService = EngineContext.Current.Resolve<Nop.Services.Customers.ICustomerService>();

                    var customer = _customerService.GetCustomerByEmail(contactRoleDto.Email);
                    if (customer == null)
                        customer = _customerService.GetCustomerByNetsuitId(Convert.ToInt32(contactRoleDto.Id));

                    if (customer != null)
                    {
                        if (customer.Email == null)
                        {
                            if (contactRoleDto.Email != null)
                            {

                                customer.Email = contactRoleDto.Email;
                            }
                        }

                        insertCustomer = customer;
                        if (contactRoleDto.custentity_website_access == true && contactRoleDto.IsInactive == false)
                            insertCustomer.Active = contactRoleDto.custentity_website_access;
                        else
                            insertCustomer.Active = false;

                        insertCustomer.Deleted = false;
                        insertCustomer.LastActivityDateUtc = DateTime.UtcNow;
                        insertCustomer.Parent = Id;
                        insertCustomer.NetsuitId = ContactId;


                        if (currentPriceLevel != null)
                            insertCustomer.PriceLevelId = currentPriceLevel.Id;
                        else
                            insertCustomer.PriceLevelId = null;


                        //Insert ItemsPricing
                        CreateListItemsPricing(companyId, customerNetSuite.ItemPricing.Link[0].Href);

                        _genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

                        if (!string.IsNullOrEmpty(contactRoleDto.FirstName))
                            _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.FirstNameAttribute, contactRoleDto.FirstName);
                        if (!string.IsNullOrEmpty(contactRoleDto.LastName))
                            _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.LastNameAttribute, contactRoleDto.LastName);
                        if (!string.IsNullOrEmpty(contactRoleDto.Phone))
                            _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.PhoneAttribute, contactRoleDto.Phone);
                        if (!string.IsNullOrEmpty(contactRoleDto.MobilePhone))
                            _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.MobilePhoneAttribute, contactRoleDto.MobilePhone);
                        if (!string.IsNullOrEmpty(contactRoleDto.OfficePhone))
                            _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.OfficePhoneAttribute, contactRoleDto.OfficePhone);

                        _customerService = EngineContext.Current.Resolve<Nop.Services.Customers.ICustomerService>();

                        _customerService.UpdateCustomer(insertCustomer);

                        #region Create Log Import Company

                        insertCustomerJson.Email = insertCustomer.Email;

                        insertCustomerJson.IsTaxExempt = insertCustomer.IsTaxExempt;
                        insertCustomerJson.AffiliateId = insertCustomer.AffiliateId;
                        insertCustomerJson.VendorId = 0;
                        insertCustomerJson.Active = insertCustomer.Active;
                        insertCustomerJson.LastActivityDateUtc = insertCustomer.LastActivityDateUtc;
                        insertCustomerJson.Parent = Id;
                        insertCustomerJson.NetsuitId = insertCustomer.NetsuitId;
                        insertCustomerJson.PriceLevelId = insertCustomer.PriceLevelId;
                        insertCustomerJson.Username = insertCustomer.Username;
                        insertCustomerJson.Id = insertCustomer.Id;


                        var jsonContactFromNetsuite = JsonConvert.SerializeObject(contactRoleDto, settings);
                        var jsonUpdated = JsonConvert.SerializeObject(insertCustomerJson, settings);
                        CreateLogImportNetsuiteData(jsonContactFromNetsuite, companyId, jsonUpdated, "Import Customers: Create Company Contact");

                        #endregion

                    }
                    else
                    {
                        var validateEmail = _customerService.GetCustomerByEmail(contactRoleDto.Email);
                        if (validateEmail == null)
                        {
                            if (contactRoleDto.Email != null)
                            {


                                // Customer Insert
                                insertCustomer.CustomerGuid = Guid.NewGuid();
                                if (validateEmail == null)
                                    insertCustomer.Username = contactRoleDto.Email;
                                else
                                {
                                    var userName = _customerService.GetCustomerByUsername(contactRoleDto.FirstName + "_" + contactRoleDto.LastName);

                                    insertCustomer.Username = contactRoleDto.FirstName + "_" + contactRoleDto.FirstName;
                                    
                                }


                                insertCustomer.Email = contactRoleDto.Email;

                                insertCustomer.IsTaxExempt = false;
                                insertCustomer.AffiliateId = 0;
                                insertCustomer.VendorId = 0;
                                insertCustomer.HasShoppingCartItems = false;
                                insertCustomer.RequireReLogin = false;
                                insertCustomer.FailedLoginAttempts = 0;

                                if (contactRoleDto.custentity_website_access == true && contactRoleDto.IsInactive == false)
                                    insertCustomer.Active = contactRoleDto.custentity_website_access;
                                else
                                    insertCustomer.Active = false;

                                insertCustomer.Deleted = false;
                                insertCustomer.IsSystemAccount = false;
                                insertCustomer.CreatedOnUtc = DateTime.UtcNow;
                                insertCustomer.LastActivityDateUtc = contactRoleDto.LastModifiedDate;
                                insertCustomer.RegisteredInStoreId = 0;
                                insertCustomer.Parent = Id;
                                insertCustomer.NetsuitId = ContactId;

                               

                                if (currentPriceLevel != null)
                                    insertCustomer.PriceLevelId = currentPriceLevel.Id;
                                else
                                    insertCustomer.PriceLevelId = null;


                                _customerService.InsertCustomer(insertCustomer);


                                CreateListItemsPricing(companyId, customerNetSuite.ItemPricing.Link[0].Href);
                                
                                _genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();

                                if (!string.IsNullOrEmpty(contactRoleDto.FirstName))
                                    _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.FirstNameAttribute, contactRoleDto.FirstName);
                                if (!string.IsNullOrEmpty(contactRoleDto.LastName))
                                    _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.LastNameAttribute, contactRoleDto.LastName);
                                if (!string.IsNullOrEmpty(contactRoleDto.Phone))
                                    _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.PhoneAttribute, contactRoleDto.Phone);
                                if (!string.IsNullOrEmpty(contactRoleDto.MobilePhone))
                                    _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.MobilePhoneAttribute, contactRoleDto.MobilePhone);
                                if (!string.IsNullOrEmpty(contactRoleDto.OfficePhone))
                                    _genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.OfficePhoneAttribute, contactRoleDto.OfficePhone);

                                _customerService.UpdateCustomer(insertCustomer);


                                #region Create Log Import Company

                                insertCustomerJson.Email = insertCustomer.Email;

                                insertCustomerJson.IsTaxExempt = insertCustomer.IsTaxExempt;
                                insertCustomerJson.AffiliateId = insertCustomer.AffiliateId;
                                insertCustomerJson.VendorId = 0;
                                insertCustomerJson.Active = insertCustomer.Active;
                                insertCustomerJson.LastActivityDateUtc = insertCustomer.LastActivityDateUtc;
                                insertCustomerJson.Parent = Id;
                                insertCustomerJson.NetsuitId = ContactId;
                                insertCustomerJson.PriceLevelId = insertCustomer.PriceLevelId;
                                insertCustomerJson.Username = insertCustomer.Username;
                                insertCustomerJson.Id = insertCustomer.Id;


                                CreateLogImportNetsuiteData(JsonConvert.SerializeObject(contactRoleDto, settings), companyId, JsonConvert.SerializeObject(insertCustomerJson, settings), "Import Customers: Update Company Contact");

                                #endregion

                            }
                        }
                        else
						{
							if (customer != null)
								insertCustomer = customer;

                            if (contactRoleDto.custentity_website_access == true && contactRoleDto.IsInactive == false)
                                insertCustomer.Active = contactRoleDto.custentity_website_access;
                            else
                                insertCustomer.Active = false;

                            insertCustomer.Deleted = false;
							insertCustomer.LastActivityDateUtc = DateTime.UtcNow;
							insertCustomer.Parent = Id;
							insertCustomer.NetsuitId = ContactId;
							insertCustomer.LastActivityDateUtc = contactRoleDto.LastModifiedDate;

                            


                            if (currentPriceLevel != null)
								insertCustomer.PriceLevelId = currentPriceLevel.Id;
							else
								insertCustomer.PriceLevelId = null;

                            _genericAttributeService = EngineContext.Current.Resolve<IGenericAttributeService>();
                            

                            CreateListItemsPricing(companyId, customerNetSuite.ItemPricing.Link[0].Href);

							_genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.FirstNameAttribute, contactRoleDto.FirstName);
							_genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.LastNameAttribute, contactRoleDto.LastName);
							_genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.PhoneAttribute, contactRoleDto.Phone);
							_genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.MobilePhoneAttribute, contactRoleDto.MobilePhone);
							_genericAttributeService.SaveAttribute(insertCustomer, NopCustomerDefaults.OfficePhoneAttribute, contactRoleDto.OfficePhone);

                            _customerService = EngineContext.Current.Resolve<Nop.Services.Customers.ICustomerService>();

                            _customerService.UpdateCustomer(insertCustomer);

                            #region Create Log Import Company

                            insertCustomerJson.Active = insertCustomer.Active;
                            insertCustomerJson.Parent = Id;
                            insertCustomerJson.NetsuitId = ContactId;
                            insertCustomerJson.LastActivityDateUtc = insertCustomer.LastActivityDateUtc;
                            insertCustomerJson.PriceLevelId = insertCustomer.PriceLevelId;
                            insertCustomerJson.Id = insertCustomer.Id;



                            CreateLogImportNetsuiteData(JsonConvert.SerializeObject(contactRoleDto, settings), companyId, JsonConvert.SerializeObject(insertCustomerJson, settings), "Import Customers: Update Company Contact");

							#endregion
						}


					}

                    if (!string.IsNullOrEmpty(insertCustomer.Email) || !string.IsNullOrEmpty(contactRoleDto.Email))
                    {
                        if (!hashCustomer.ContainsKey(insertCustomer.Email))
                            hashCustomer.Add(insertCustomer.Email, insertCustomer.Id);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Warning("ImportCustomerError::   insert Customer:: idCustomerNP " + Id + " CustomerName " + insertCustomer.Email + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

            return insertCustomer;
        }
        private void UpdateCustomer(ContactObject contactRoleDto, Customer customer, Company company, CustomerDto customerNetSuite)
        {
            try
            {
                if (customer != null)
                {
                    PriceLevel currentPriceLevel = null;

                    if (!string.IsNullOrEmpty(customerNetSuite.PriceLevel?.Id))
                        currentPriceLevel = _priceLevelService.GetExistsPriceLevel(customerNetSuite.PriceLevel?.Id);

                    if (customer.Email == null && customer.NetsuitId == Convert.ToInt32(contactRoleDto.Id))
                        customer.Email = contactRoleDto.Email;

                    if (contactRoleDto.custentity_website_access == true && contactRoleDto.IsInactive == false)
                        customer.Active = contactRoleDto.custentity_website_access;
                    else
                        customer.Active = false;

                    customer.Deleted = false;
                    customer.LastActivityDateUtc = DateTime.UtcNow;


                    if (currentPriceLevel != null)
                        customer.PriceLevelId = currentPriceLevel.Id;
                    else
                        customer.PriceLevelId = null;

                    CreateListItemsPricing(company.Id, customerNetSuite.ItemPricing.Link[0].Href);

                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.FirstNameAttribute, contactRoleDto.FirstName);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.LastNameAttribute, contactRoleDto.LastName);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.PhoneAttribute, contactRoleDto.Phone);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.MobilePhoneAttribute, contactRoleDto.MobilePhone);
                    _genericAttributeService.SaveAttribute(customer, NopCustomerDefaults.OfficePhoneAttribute, contactRoleDto.OfficePhone);

                    var billingAddress = _companyService.GetAllCompanyAddressMappingsById(company.Id);
                    foreach (var item in billingAddress)
                    {
                        if (item.IsBilling)
                        {
                            customer.BillingAddressId = item.AddressId;
                            customer.BillingAddress = item.Address;
                        }
                        if (item.IsShipping)
                        {
                            customer.ShippingAddressId = item.AddressId;
                            customer.ShippingAddress = item.Address;
                        }
                    }

                    _customerService.UpdateCustomer(customer);

                    if (!hashCustomer.ContainsKey(customer.Email))
                        hashCustomer.Add(customer.Email, customer.Id);
                }
            }
            catch (Exception ex)
            {
                _notificationService = EngineContext.Current.Resolve<INotificationService>();
                _notificationService.ErrorNotification(ex.Message);

                _logger = EngineContext.Current.Resolve<ILogger>();
                _logger.Warning("ImportCustomerError:: UpdateCustomer:: customer Id" + customer.Id + " Contact Netsuite: " + contactRoleDto.Id + " -CompanyName: " + company.CompanyName + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        #endregion

        #region Company
        private Company CreateCompanyInfo(CustomerDto customerDTO)
        {
            // Customer Insert
            Company company = new Company();
            PriceLevel currentPriceLevel = null;
            var _priceLevelService = EngineContext.Current.Resolve<IPriceLevelService>();

            if (!string.IsNullOrEmpty(customerDTO.PriceLevel?.Id))
                currentPriceLevel = _priceLevelService.GetExistsPriceLevel(customerDTO.PriceLevel?.Id);

            company.CompanyName = customerDTO.CompanyName;
            company.Email = customerDTO.Email;

            if (customerDTO.PrimaryLocation != null)
                company.PrimaryLocation = customerDTO.PrimaryLocation.RefName;

            company.Balance = customerDTO.Balance;
            company.ConsoleBalance = customerDTO.ConsoleBalance;
            company.DefaultAddress = customerDTO.DefaultAddress;
            company.EntityStatus = customerDTO.EntityStatus.RefName;
            company.EntityStatusId = customerDTO.EntityStatus.Id;
            company.NetsuiteId = customerDTO.Id;
            company.LastModifiedDate = customerDTO.LastModifiedDate;
            company.Phone = customerDTO.Phone;
            company.PONumberReq = customerDTO.PORequired;

            company.custentity_tj_exempt_customer = customerDTO.custentity_tj_exempt_customer;
            if (customerDTO.custentity_tj_exempt_customer_states != null)
                company.custentity_tj_exempt_customer_states = customerDTO.custentity_tj_exempt_customer_states.RefName;

            if (customerDTO.custentity_tj_exempt_customer == true)
            {

            }

            if (customerDTO.custentity_tj_exempt_customer_states != null)
            {

            }
            if (customerDTO.Terms != null)
            {
                company.BillingTerms = customerDTO.Terms?.Id;
                company.Terms = customerDTO.Terms?.RefName;
            }

            if (currentPriceLevel != null)
                company.PriceLevelId = currentPriceLevel.Id;
            else
                company.PriceLevelId = null;

            _companyService = EngineContext.Current.Resolve<Nop.Services.Customers.ICompanyService>();

            _companyService.InsertCompany(company);



            #region Create Log Import Company
            Company companyJson = new Company();

            companyJson.CompanyName = company.CompanyName;
            companyJson.Email = company.Email;

            companyJson.PrimaryLocation = company.PrimaryLocation;

            companyJson.Balance = company.Balance;
            companyJson.ConsoleBalance = company.ConsoleBalance;
            companyJson.DefaultAddress = company.DefaultAddress;
            companyJson.EntityStatus = company.EntityStatus;
            companyJson.EntityStatusId = company.EntityStatusId;
            companyJson.NetsuiteId = company.NetsuiteId;
            companyJson.LastModifiedDate = company.LastModifiedDate;
            companyJson.Phone = company.Phone;
            companyJson.PONumberReq = company.PONumberReq;

            companyJson.custentity_tj_exempt_customer = company.custentity_tj_exempt_customer;
            companyJson.custentity_tj_exempt_customer_states = company.custentity_tj_exempt_customer_states;
            companyJson.BillingTerms = company.BillingTerms;
            companyJson.Terms = company.Terms;                
            companyJson.PriceLevelId = company.PriceLevelId;
            companyJson.Id = company.Id;

            var settings = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            CreateLogImportNetsuiteData(JsonConvert.SerializeObject(customerDTO, settings), company.Id, JsonConvert.SerializeObject(companyJson, settings), "Import Customers: Import Company Info");

            #endregion



            return company;
        }

        private void UpdateCompany(CustomerDto customerDTO, List<Company> Companies, int id)
        {
            try
            {
                var _priceLevelService = EngineContext.Current.Resolve<IPriceLevelService>();
                PriceLevel currentPriceLevel = null;

                if (!string.IsNullOrEmpty(customerDTO.PriceLevel?.Id))
                    currentPriceLevel = _priceLevelService.GetExistsPriceLevel(customerDTO.PriceLevel?.Id);

                foreach (var comp in Companies)
                {
                    var _companyService = EngineContext.Current.Resolve<Nop.Services.Customers.ICompanyService>();


                    var company = _companyService.GetCompanyById(comp.Id);
                    company.CompanyName = customerDTO.CompanyName;
                    if (!string.IsNullOrEmpty(customerDTO.Email))
                        company.Email = customerDTO.Email;
                    else
                        company.Email = customerDTO.EmailForCustomerBiling;

                    if (customerDTO.PrimaryLocation != null)
                        company.PrimaryLocation = customerDTO.PrimaryLocation.RefName;

                    company.Balance = customerDTO.Balance;
                    company.ConsoleBalance = customerDTO.ConsoleBalance;
                    company.DefaultAddress = customerDTO.DefaultAddress;
                    company.EntityStatus = customerDTO.EntityStatus.RefName;
                    company.EntityStatusId = customerDTO.EntityStatus.Id;
                    company.NetsuiteId = customerDTO.Id;
                    company.LastModifiedDate = customerDTO.LastModifiedDate;

                    company.PONumberReq = customerDTO.PORequired;

                    company.Phone = customerDTO.Phone;
                    if (customerDTO.parent != null)
                        company.Parent_Id = Convert.ToInt32(customerDTO.parent?.Id);

                    if (currentPriceLevel != null)
                        company.PriceLevelId = currentPriceLevel.Id;
                    else
                        company.PriceLevelId = null;

                    if (customerDTO.Terms != null)
                    {
                        company.BillingTerms = customerDTO.Terms?.Id;
                        company.Terms = customerDTO.Terms?.RefName;
                    }

                    company.custentity_tj_exempt_customer = customerDTO.custentity_tj_exempt_customer;

                    if (customerDTO.custentity_tj_exempt_customer_states != null)
                        company.custentity_tj_exempt_customer_states = customerDTO.custentity_tj_exempt_customer_states.RefName;

                    _companyService.UpdateCompany(company);

                    #region Create Log Import Company

                    Company companyJson = new Company();
                    companyJson.Id = company.Id;
                    companyJson.CompanyName = company.CompanyName;
                    companyJson.Email = company.Email;

                    companyJson.PrimaryLocation = company.PrimaryLocation;

                    companyJson.Balance = company.Balance;
                    companyJson.ConsoleBalance = company.ConsoleBalance;
                    companyJson.DefaultAddress = company.DefaultAddress;
                    companyJson.EntityStatus = company.EntityStatus;
                    companyJson.EntityStatusId = company.EntityStatusId;
                    companyJson.NetsuiteId = company.NetsuiteId;
                    companyJson.LastModifiedDate = company.LastModifiedDate;
                    companyJson.Phone = company.Phone;
                    companyJson.PONumberReq = company.PONumberReq;

                    companyJson.custentity_tj_exempt_customer = company.custentity_tj_exempt_customer;
                    companyJson.custentity_tj_exempt_customer_states = company.custentity_tj_exempt_customer_states;
                    companyJson.BillingTerms = company.BillingTerms;
                    companyJson.Terms = company.Terms;
                    companyJson.PriceLevelId = company.PriceLevelId;
                    companyJson.Id = company.Id;

                    var settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    CreateLogImportNetsuiteData(JsonConvert.SerializeObject(customerDTO, settings), company.Id, JsonConvert.SerializeObject(companyJson, settings), "Import Customers: Update Company Info");

                    #endregion

                }
            }
            catch (Exception ex)
            {
                _notificationService = EngineContext.Current.Resolve<INotificationService>();

                _notificationService.ErrorNotification(ex.Message);

                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Warning("ImportCustomerError::  UpdateCompany:: customerDTOName " + customerDTO.CompanyName + " customerDTOId " + customerDTO.Id + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

        }

        private void InsertCompanyAddress(char delimitador, int Id, CustomerDto customerDTO, Company company)
        {
            var ListAdress = GetAddressBook(Id);

            // Create Address
            foreach (var addressBook in ListAdress.Items)
            {
                foreach (var address in addressBook.Links)
                {
                    string[] id = address.Href.Split(delimitador);
                    var ultimoId = Convert.ToInt32(id[id.Length - 1]);

                    var AddressBookDTO = GetAddress(Id, ultimoId);
                    var GetAddressBook = GetAddressBookAddress(Id, ultimoId);
                    var country = _countryService.GetCountryByTwoLetterIsoCode(GetAddressBook.Country?.Id);
                    var state = _stateProvinceService.GetStateProvinceByAbbreviation(GetAddressBook.State);

                    //Insert Address
                    Address insertAddress = new Address();

                    if (customerDTO.Email != null)
                        insertAddress.Email = customerDTO.Email;
                    else
                    {
                        if (company.Email != null)
                            insertAddress.Email = company.Email;
                        else
                            insertAddress.Email = "info@yourstore.com";
                    }

                    insertAddress.Company = customerDTO.CompanyName;
                    insertAddress.Address1 = GetAddressBook.Addr1;
                    insertAddress.Address2 = GetAddressBook.Addr2;
                    insertAddress.CreatedOnUtc = DateTime.UtcNow;
                    insertAddress.NetsuitId = AddressBookDTO.Id;

                    insertAddress.Country = country;
                    insertAddress.StateProvince = state;
                    insertAddress.CountryId = country?.Id;
                    insertAddress.StateProvinceId = state?.Id;
                    insertAddress.City = GetAddressBook.City;
                    insertAddress.ZipPostalCode = GetAddressBook.Zip;

                    if (GetAddressBook.AddrPhone != null)
                        insertAddress.PhoneNumber = GetAddressBook.AddrPhone;
                    else
                        insertAddress.PhoneNumber = "0";

                    if (customerDTO.CompanyName != null)
                        insertAddress.FirstName = customerDTO.CompanyName;
                    else
                        insertAddress.FirstName = GetAddressBook.Addr1;

                    if (GetAddressBook.City != null)
                        insertAddress.LastName = GetAddressBook.City;
                    else
                        insertAddress.LastName = customerDTO.CompanyName;

                    _addressService.InsertAddress(insertAddress);

                    CompanyAddresses CompanyAddress = new CompanyAddresses();
                    CompanyAddress.CompanyId = company.Id;
                    CompanyAddress.AddressId = insertAddress.Id;

                    CompanyAddress.IsBilling = AddressBookDTO.DefaultBilling;
                    CompanyAddress.IsShipping = AddressBookDTO.DefaultShipping;
                    CompanyAddress.Label = AddressBookDTO.Label;

                    //if (customerDTO.Deliveryroute != null)
                    //{
                    if (GetAddressBook.APPROVEDROUTEDELIVERY != null)
                    {
                        if (GetAddressBook.APPROVEDROUTEDELIVERY?.Id != null)
                        {
                            CompanyAddress.DeliveryRoute = true;
                            CompanyAddress.DeliveryRouteId = GetAddressBook?.APPROVEDROUTEDELIVERY?.Id;
                            CompanyAddress.DeliveryRouteName = GetAddressBook?.APPROVEDROUTEDELIVERY?.ReferenceName;
                        }
                        else
                            CompanyAddress.DeliveryRouteId = "0";
                    }
                    else
                        CompanyAddress.DeliveryRouteId = "0";


                    _companyService.InsertCompanyAddressMapping(CompanyAddress);

                    #region Create Log Import Company
                    var settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    CreateLogImportNetsuiteData(JsonConvert.SerializeObject(GetAddressBook, settings), company.Id, JsonConvert.SerializeObject(CompanyAddress, settings), "Import Customers: Update Company - Address");


                    #endregion

                }
            }
        }

        #endregion

        #region Methods NetSuite Data
        private CustomersObject GetCustomers(string delimit, string activeImport)
        {
            var body = @"{""q"": ""SELECT *  FROM customer c  where  (" + activeImport + ")  and isinactive='F'";
            var data = _connectionService.GetConnection("/query/v1/suiteql" + delimit, "POST", body);//31, 13, 7 
            return JsonConvert.DeserializeObject<CustomersObject>(data);
        }

        private CustomerDto GetCustomer(int Id)
        {
            var GetData = _connectionService.GetConnection("/record/v1/customer/" + Id, "GET");
            var ListCustomer = JsonConvert.DeserializeObject<CustomerDto>(GetData);
            return ListCustomer;
        }

        private void CreateListItemsPricing(int companyId, string url)
        {
            string log = string.Empty;
            try
            {
                _itemPricingService = EngineContext.Current.Resolve<IItemPricingService>();

                //Delete old info
                _itemPricingService.DeleteItemPricingByCompanyId(companyId);

                var GetData = _connectionService.GetConnection(url.Substring(url.IndexOf("/record"), url.Length - url.IndexOf("/record")), "GET");
                var ListItems = JsonConvert.DeserializeObject<dynamic>(GetData);

                foreach (var item in ListItems.items)
                {
                    string urlItemPricing = item.links[0].href;
                    var GetDataItemPricing = _connectionService.GetConnection(urlItemPricing.Substring(urlItemPricing.IndexOf("/record"), urlItemPricing.Length - urlItemPricing.IndexOf("/record")), "GET");
                    log = GetDataItemPricing;
                    var infoItem = JsonConvert.DeserializeObject<dynamic>(GetDataItemPricing);
                    string idItemInventory = infoItem.item.id;
                    if (infoItem.level != null)
                    {
                        string levelId = infoItem.level?["id"];
                        var levelName = infoItem.level?["refName"];
                        //decimal priceItem = infoItem.price;
                        _priceLevelService = EngineContext.Current.Resolve<IPriceLevelService>();
                        //_productRespository = EngineContext.Current.Resolve<Product>();

                        var priceLevel = _priceLevelService.GetExistsPriceLevel(levelId);
                        var currentProduct = _productRespository.GetByWhere(s => s.IdInventoryItem.Equals(idItemInventory));

                        if (levelId != "-1")
                        {

                            //Insert new Info
                            //var priceLevelqty = _priceLevelService.
                            if (currentProduct != null && priceLevel != null)
                            {
                                var priceItem = currentProduct?.PriceByQtyProduct.Where(r => r.PriceLevelId == priceLevel.Id).FirstOrDefault();
                                if (priceItem != null)
                                {

                                    _itemPricingService.InsertItemPricing(new ItemPricing()
                                    {
                                        CompanyId = companyId,
                                        Price = priceItem.Price,
                                        ProductId = currentProduct.Id,
                                        PriceLevel = priceItem.PriceLevelId
                                    });

                                }
                            }
                        }
                        else
                        {
                            if (currentProduct != null)
                            {
                                if (infoItem?["price"] != null)
                                {

                                    _itemPricingService.InsertItemPricing(new ItemPricing()
                                    {
                                        CompanyId = companyId,
                                        Price = infoItem?["price"],
                                        ProductId = currentProduct.Id,
                                        PriceLevel = -1
                                    });

                                }
                            }
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService = EngineContext.Current.Resolve<INotificationService>();
                _logger = EngineContext.Current.Resolve<ILogger>();
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportCustomerError:: CreateListItemsPricing:: companyId" + companyId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private CustomersObject GetAddressBook(int Id)
        {
            var GetData = _connectionService.GetConnection("/record/v1/customer/" + Id + "/addressBook", "GET");
            var ListAdress = JsonConvert.DeserializeObject<CustomersObject>(GetData);
            return ListAdress;
        }

        private AddressBookObject GetAddress(int Id, int IdContact)
        {
            var GetData = _connectionService.GetConnection("/record/v1/customer/" + Id + "/addressBook/" + IdContact, "GET");
            var Address = JsonConvert.DeserializeObject<AddressBookObject>(GetData);
            return Address;
        }

        private AddressBookAddress GetAddressBookAddress(int Id, int IdContact)
        {
            var GetData = _connectionService.GetConnection("/record/v1/customer/" + Id + "/addressBook/" + IdContact + "/addressBookAddress", "GET");
            var Address = JsonConvert.DeserializeObject<AddressBookAddress>(GetData);
            return Address;
        }

        private CustomersObject GetContactsRole(int Id)
        {
            var GetData = _connectionService.GetConnection("/record/v1/customer/" + Id + "/contactRoles", "GET");
            var ListContact = JsonConvert.DeserializeObject<CustomersObject>(GetData);
            return ListContact;
        }

        private ContactObject GetContact(int IdContact)
        {
            var GetData = _connectionService.GetConnection("/record/v1/contact/" + IdContact, "GET");
            var Contact = JsonConvert.DeserializeObject<ContactObject>(GetData);
            return Contact;
        }

        private CustomersObject GetCustomers(string delimit, string LastExecutionDate, string activeImport)
        {
            var body = @"{""q"": ""SELECT c.* FROM customer c WHERE  (" + activeImport + ")  and isinactive='F' and c.lastModifiedDate>='" + LastExecutionDate + "'\"}";
            var data = _connectionService.GetConnection("/query/v1/suiteql" + delimit, "POST", body);
            return JsonConvert.DeserializeObject<CustomersObject>(data);
        }
		#endregion

		#region Other Methods
		
		private void CustomerRoleAccountUpdateList()
        {
            try
            {
                var customer = _customerService.GetAllCustomers();
                foreach (var insertCustomer in customer)
                {
                    if (insertCustomer.NetsuitId != 0)
                    {
                        var customerRole = _customerService.GetCustomerCustomerOrleById(insertCustomer.Id);
                        var register = false;
                        var accRole = false;

                        if (customerRole.Count > 0)
                        {
                            foreach (var item in customerRole)
                            {
                                if (item.CustomerRoleId == RegisterRole)
                                    register = true;

                                if (item.CustomerRoleId == AccountCustomerRole)
                                    accRole = true;
                            }

                        }

                        if (!register)
                        {
                            CustomerCustomerRoleMapping CustomerRole = new CustomerCustomerRoleMapping();
                            CustomerRole.CustomerId = insertCustomer.Id;
                            CustomerRole.CustomerRoleId = RegisterRole;
                            _customerCustomerRoleMappingRepository.Insert(CustomerRole);
                        }

                        if (!accRole)
                        {
                            CustomerCustomerRoleMapping CustomerRoleAccount = new CustomerCustomerRoleMapping();
                            CustomerRoleAccount.CustomerId = insertCustomer.Id;
                            CustomerRoleAccount.CustomerRoleId = AccountCustomerRole;
                            _customerCustomerRoleMappingRepository.Insert(CustomerRoleAccount);
                        }

                    }

                }
            }
            catch (Exception ex)
            {
                _notificationService = EngineContext.Current.Resolve<INotificationService>();
                _logger = EngineContext.Current.Resolve<ILogger>();
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportCustomerError::   CustomerRoleAccountUpdateList:: Error CustomerRoleAccountUpdateList - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

        }

        private void CreateLogImportNetsuiteData(string DataFromNetsuite, int companyId, string DataUpdatedNetsuite, string ImportName)
        {
            _logImportNetsuite = EngineContext.Current.Resolve<ILogImportNetsuiteService>();

            LogNetsuiteImport logNetsuiteImport = new LogNetsuiteImport();
            logNetsuiteImport.DateCreate = DateTime.Now;
            logNetsuiteImport.DataFromNetsuite = DataFromNetsuite;
            logNetsuiteImport.DataUpdatedNetsuite = DataUpdatedNetsuite;
            logNetsuiteImport.ImportName = ImportName;
            logNetsuiteImport.Message = "";
            logNetsuiteImport.Type = 1;
            logNetsuiteImport.RegisterId = companyId;
            logNetsuiteImport.LastExecutionDateGeneral = LastExecutionDateGeneral;
            _logImportNetsuite.InsertLog(logNetsuiteImport);
        }

        #endregion

        #region Contacts 
        private string UpdateContactNetsuite(ContactUpdateNetsuite contactUpdate, int contactId)
        {
            var jsonString = JsonConvert.SerializeObject(contactUpdate);

			var data = _connectionService.GetConnection("/record/v1/contact/" + contactId, "PATCH", jsonString);
			return data;
        }
        #endregion
    }

}
