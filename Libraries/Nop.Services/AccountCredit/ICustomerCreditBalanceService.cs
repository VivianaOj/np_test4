using Nop.Core.Domain.Invoice;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.AccountCredit
{
    public partial interface ICustomerCreditBalanceService
    {
        //  CustomerAccountCredit InsertInvoice(CustomerAccountCredit invoice);
        void InsertCustomerCreditBalance(CustomerAccountCredit invoice);

        void UpdateCustomerCreditBalance(CustomerAccountCredit credit);
        IList<CustomerAccountCredit> GetCustomerCreditBalance(int CompanyId);
        CustomerAccountCredit GetCustomerCreditBalanceById(int CompanyId, int NetsuiteId);
        void DeleteCustomerCreditBalance(CustomerAccountCredit invoice);

        void DeleteCustomerCreditBalance(int CompanyId);
    }
}
