using Newtonsoft.Json;
using Nop.Core.Domain.Invoice;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Services.AccountCredit;
using Nop.Services.Invoices;
using Nop.Services.Logging;
using Nop.Services.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Order
{
	public class CreditManageService: ICreditManageService
	{
        private readonly ICustomerAccountCreditApplyService _creditApply;
        private readonly ICustomerCreditBalanceService _credit;
        private readonly ILogger _logger;
        private readonly INotificationService _notificationService;
        private readonly IConnectionServices _connectionService;
        public CreditManageService(ICustomerCreditBalanceService credit, ILogger logger, INotificationService notificationService, IConnectionServices connectionService)
		{
            _credit = credit;
            _logger = logger;
            _notificationService = notificationService;
            _connectionService = connectionService;
        }

        public void GetCustomerBalance(int companyId, string LastExecutionDateGeneral)
        {
            Hashtable hashCustomer = new Hashtable();

            var GetCustomerAccountCreditApply = _credit.GetCustomerCreditBalance(companyId).ToList();

            //type=1 credit memo, type=2 customer deposit, type =3 customer payment
            GetCreditMemosByCompanyId(companyId, GetCustomerAccountCreditApply, hashCustomer, LastExecutionDateGeneral);
            GetCustomerDepositeByCompanyId(companyId, GetCustomerAccountCreditApply, hashCustomer, LastExecutionDateGeneral);
            GetCustomerPaymentsByCompanyId(companyId, GetCustomerAccountCreditApply, hashCustomer, LastExecutionDateGeneral);

            InactiveCredits(GetCustomerAccountCreditApply, hashCustomer, LastExecutionDateGeneral);

        }
        private void GetCustomerPaymentsByCompanyId(int CompanyId, List<CustomerAccountCredit> CustomerAccountCreditApply, Hashtable hashCustomer, string LastExecutionDateGeneral)
        {
            decimal total = 0;
            var customerPayment = GetCustomerPaymentByCustomer(CompanyId);

            if (customerPayment != null)
            {
                foreach (var item in customerPayment.Items)
                {
                    var creditMemoValue = GetCustomerPaymentId(item.Id);


                    if (CustomerAccountCreditApply == null)
                        CustomerAccountCreditApply = new List<CustomerAccountCredit>();

                    var CreditMemoSaved = CustomerAccountCreditApply.Where(r => r.NetsuiteId == item.Id).OrderByDescending(r => r.Id).FirstOrDefault();

                    if (CreditMemoSaved == null)
                    {
                        CustomerAccountCredit creditMemo = new CustomerAccountCredit();

                        if (creditMemoValue.unapplied > 0)
                        {
                            var totalImport = (decimal)creditMemoValue.unapplied;

                            creditMemo.AccountCredit = totalImport;
                            creditMemo.DateSync = DateTime.Now;
                            creditMemo.CompanyId = CompanyId;
                            creditMemo.Name = "Customer Payments";
                            creditMemo.Transid = item.TranId;
                            creditMemo.NetsuiteId = item.Id;
                            creditMemo.lastModifiedDate = item.Lastmodifieddate;
                            creditMemo.TotalApply = 0;
                            creditMemo.Type = 3;
                            creditMemo.DateApplyUpdate = DateTime.Now;
                            creditMemo.IsInActive = false;

                            _logger.Warning("Insert CPt " + creditMemo.Transid + ", Customer: " + CompanyId);

                            try
                            {
                                _credit.InsertCustomerCreditBalance(creditMemo);
                                total = total + creditMemo.AccountCredit;

                                if (!hashCustomer.ContainsKey(creditMemo.Id))
                                    hashCustomer.Add(creditMemo.Id, item.TranId);


                            }
                            catch (Exception ex)
                            {
                                _notificationService.ErrorNotification(ex.Message);
                                _logger.Warning("ImportOrderError:: GetCustomerPaymentsByCompanyId  Saved:: CompanyId" + CompanyId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

                            }
                        }
                    }
                    else
                    {
                        if (creditMemoValue.unapplied > 0)
                        {
                            var totalImport = (decimal)creditMemoValue.unapplied;

                            CreditMemoSaved.AccountCredit = totalImport;
                            CreditMemoSaved.DateSync = DateTime.Now;

                            CreditMemoSaved.Name = "Customer Payment";
                            CreditMemoSaved.Transid = item.TranId;
                            CreditMemoSaved.NetsuiteId = item.Id;
                            CreditMemoSaved.lastModifiedDate = item.Lastmodifieddate;
                            CreditMemoSaved.TotalApply = 0;
                            CreditMemoSaved.Type = 3;
                            CreditMemoSaved.DateApplyUpdate = DateTime.Now;
                            CreditMemoSaved.IsInActive = false;

                            try
                            {
                                _credit.UpdateCustomerCreditBalance(CreditMemoSaved);
                                total = total + CreditMemoSaved.AccountCredit;

                                if (!hashCustomer.ContainsKey(CreditMemoSaved.Id))
                                    hashCustomer.Add(CreditMemoSaved.Id, item.TranId);


                            }
                            catch (Exception ex)
                            {
                                _notificationService.ErrorNotification(ex.Message);
                                _logger.Warning("ImportOrderError:: GetCustomerPaymentsByCompanyId :: CompanyId" + CompanyId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                            }
                        }

                    }
                }
            }
        }

        private void GetCustomerDepositeByCompanyId(int CompanyId, List<CustomerAccountCredit> CustomerAccountCreditApply, Hashtable hashCustomer, string LastExecutionDateGeneral)
        {
            decimal total = 0;
            var CustomerDeposite = GetCustomerDepositeByCustomer(CompanyId);

            if (CustomerDeposite != null)
            {
                foreach (var item in CustomerDeposite.Items)
                {
                    if (CustomerAccountCreditApply == null)
                        CustomerAccountCreditApply = new List<CustomerAccountCredit>();

                    var CreditMemoSaved = CustomerAccountCreditApply.Where(r => r.NetsuiteId == item.Id).OrderByDescending(r => r.Id).FirstOrDefault();

                    if (CreditMemoSaved == null)
                    {
                        CustomerAccountCredit creditMemo = new CustomerAccountCredit();
                        if (item.custbody_deposit_unapplied > 0)
                        {
                            var totalImport = (decimal)item.custbody_deposit_unapplied;

                            creditMemo.AccountCredit = totalImport;
                            creditMemo.DateSync = DateTime.Now;
                            creditMemo.CompanyId = CompanyId;
                            creditMemo.Name = "Customer Deposit";
                            creditMemo.Transid = item.TranId;
                            creditMemo.NetsuiteId = item.Id;
                            creditMemo.lastModifiedDate = item.Lastmodifieddate;
                            creditMemo.TotalApply = 0;
                            creditMemo.Type = 2;
                            creditMemo.DateApplyUpdate = DateTime.Now;
                            creditMemo.IsInActive = false;

                            _logger.Warning("Insert CD " + creditMemo.Transid + ", Customer: " + CompanyId);

                            try
                            {
                                _credit.InsertCustomerCreditBalance(creditMemo);
                                total = total + creditMemo.AccountCredit;

                                if (!hashCustomer.ContainsKey(creditMemo.Id))
                                    hashCustomer.Add(creditMemo.Id, item.TranId);
                            }
                            catch (Exception ex)
                            {
                                _notificationService.ErrorNotification(ex.Message);
                                _logger.Warning("ImportOrderError:: GetCustomerDepositeByCompanyId  Saved:: CompanyId" + CompanyId + "Customer Deposit TranId" + item.TranId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                            }
                        }
                    }
                    else
                    {
                        if (item.custbody_deposit_unapplied > 0)
                        {
                            var totalImport = (decimal)item.custbody_deposit_unapplied;

                            CreditMemoSaved.AccountCredit = totalImport;
                            CreditMemoSaved.DateSync = DateTime.Now;

                            CreditMemoSaved.Name = "Customer Deposit";
                            CreditMemoSaved.Transid = item.TranId;
                            CreditMemoSaved.NetsuiteId = item.Id;
                            CreditMemoSaved.lastModifiedDate = item.Lastmodifieddate;
                            CreditMemoSaved.TotalApply = 0;
                            CreditMemoSaved.Type = 2;
                            CreditMemoSaved.DateApplyUpdate = DateTime.Now;
                            CreditMemoSaved.IsInActive = false;

                            try
                            {
                                _credit.UpdateCustomerCreditBalance(CreditMemoSaved);
                                total = total + CreditMemoSaved.AccountCredit;

                                if (!hashCustomer.ContainsKey(CreditMemoSaved.Id))
                                    hashCustomer.Add(CreditMemoSaved.Id, item.TranId);


                            }
                            catch (Exception ex)
                            {
                                _notificationService.ErrorNotification(ex.Message);
                                _logger.Warning("ImportOrderError:: GetCustomerDepositeByCompanyId :: CompanyId" + CompanyId + "Customer Deposit TranId" + item.TranId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

                            }
                        }

                    }
                }
            }
        }

        private void GetCreditMemosByCompanyId(int CompanyId, List<CustomerAccountCredit> CustomerAccountCreditApply, Hashtable hashCustomer,string LastExecutionDateGeneral)
        {

            decimal total = 0;
            var CreditMemos = GetCreditMemosByCustomer(CompanyId);

            if (CreditMemos != null)
            {
                foreach (var item in CreditMemos.Items)
                {
                    var creditMemoValue = GetCreditMemoId(item.Id);


                    if (CustomerAccountCreditApply == null)
                        CustomerAccountCreditApply = new List<CustomerAccountCredit>();

                    var CreditMemoSaved = CustomerAccountCreditApply.Where(r => r.NetsuiteId == item.Id).OrderByDescending(r => r.Id).FirstOrDefault();

                    if (CreditMemoSaved == null)
                    {
                        CustomerAccountCredit creditMemo = new CustomerAccountCredit();
                        if (creditMemoValue.unapplied > 0)
                        {
                            var totalImport = (decimal)creditMemoValue.unapplied;

                            creditMemo.AccountCredit = totalImport;
                            creditMemo.DateSync = DateTime.Now;
                            creditMemo.CompanyId = CompanyId;
                            creditMemo.Name = "Credit Memo";
                            creditMemo.Transid = item.TranId;
                            creditMemo.NetsuiteId = item.Id;
                            creditMemo.lastModifiedDate = item.Lastmodifieddate;
                            creditMemo.TotalApply = 0;
                            creditMemo.Type = 1;
                            creditMemo.IsInActive = false;
                            creditMemo.DateApplyUpdate = DateTime.Now;

                            _logger.Warning("Insert CM " + creditMemo.Transid + ", Customer: " + CompanyId);

                            try
                            {
                                _credit.InsertCustomerCreditBalance(creditMemo);
                                total = total + creditMemo.AccountCredit;

                                if (!hashCustomer.ContainsKey(creditMemo.Id))
                                    hashCustomer.Add(creditMemo.Id, item.TranId);
                            }
                            catch (Exception ex)
                            {
                                _notificationService.ErrorNotification(ex.Message);
                                _logger.Warning("ImportOrderError:: GetCreditMemosByCompanyId  :: CompanyId" + CompanyId + "Customer Deposit TranId" + item.TranId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

                            }
                        }
                    }
                    else
                    {
                        if (creditMemoValue.unapplied > 0)
                        {
                            var totalImport = (decimal)creditMemoValue.unapplied;

                            CreditMemoSaved.AccountCredit = totalImport;
                            CreditMemoSaved.DateSync = DateTime.Now;
                            CreditMemoSaved.IsInActive = false;
                            CreditMemoSaved.Name = "Credit Memo";
                            CreditMemoSaved.Transid = item.TranId;
                            CreditMemoSaved.NetsuiteId = item.Id;
                            CreditMemoSaved.lastModifiedDate = item.Lastmodifieddate;
                            CreditMemoSaved.TotalApply = 0;
                            CreditMemoSaved.Type = 1;
                            CreditMemoSaved.DateApplyUpdate = DateTime.Now;

                            try
                            {
                                _credit.UpdateCustomerCreditBalance(CreditMemoSaved);
                                total = total + CreditMemoSaved.AccountCredit;

                                if (!hashCustomer.ContainsKey(CreditMemoSaved.Id))
                                    hashCustomer.Add(CreditMemoSaved.Id, item.TranId);


                            }
                            catch (Exception ex)
                            {
                                _notificationService.ErrorNotification(ex.Message);
                                _logger.Warning("ImportOrderError:: GetCreditMemosByCompanyId Saved :: CompanyId" + CompanyId + "Customer Deposit TranId" + item.TranId + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                            }
                        }
                    }
                }
            }
        }

        private void InactiveCredits(List<CustomerAccountCredit> CustomerAccountCreditApply, Hashtable hashCustomer, string LastExecutionDateGeneral)
        {

            foreach (var item in CustomerAccountCreditApply)
            {
                try
                {
                    if (!hashCustomer.ContainsKey(item.Id))
                    {
                        item.IsInActive = true;
                        _credit.UpdateCustomerCreditBalance(item);
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ErrorNotification(ex.Message);
                    _logger.Warning("ImportOrderError:: InactiveCredits  ::  item.Name" + item.Name + " item.TotalAppl" + item.TotalApply + " Transid" + item.Transid + " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

                }
            }
        }

		#region Netsuite connection and get data
		private TransactionListDto GetCustomerPaymentByCustomer(int companyId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS NetsuiteID, t.custbody10, t.custbody_website_order_number, t.status,t.custbody_website_order_status, t.*  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustPymt' and ( t.status='A' or  t.status='B')  and c.id =" + companyId + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetCustomerDepositeByCustomer(int companyId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS NetsuiteID, t.custbody10, t.custbody_website_order_number, t.status,t.custbody_website_order_status, t.*  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustDep' and ( t.status='A' or  t.status='B')  and c.id =" + companyId + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private TransactionListDto GetCreditMemosByCustomer(int companyId)
        {
            var body = @"{""q"": ""SELECT c.email AS email, c.companyName AS company, t.tranId AS document, t.tranDate AS date, c.id AS customerId, t.id AS NetsuiteID, t.custbody10, t.custbody_website_order_number, t.status,t.custbody_website_order_status, t.*  FROM customer c, transaction t WHERE t.entity = c.id AND t.type = 'CustCred' and t.status='A'  and c.id =" + companyId + "'\" }";
            var data = _connectionService.GetConnection("/query/v1/suiteql", "POST", body);
            return JsonConvert.DeserializeObject<TransactionListDto>(data);
        }

        private CreditMemo GetCreditMemoId(int Id)
        {
            var data = _connectionService.GetConnection("/record/v1/creditmemo/" + Id, "GET");
            return JsonConvert.DeserializeObject<CreditMemo>(data);
        }

        private CreditMemo GetCustomerPaymentId(int Id)
        {
            var data = _connectionService.GetConnection("/record/v1/customerPayment/" + Id, "GET");
            return JsonConvert.DeserializeObject<CreditMemo>(data);
        }
        #endregion
    }
}
