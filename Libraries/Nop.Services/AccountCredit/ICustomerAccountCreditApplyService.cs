using System.Collections.Generic;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;

namespace Nop.Services.Invoices
{
    public partial interface ICustomerAccountCreditApplyService
    {
        //void InsertInvoice(CustomerAccountCreditApply credit);

        //IList<CustomerAccountCreditApply> GetCustomerAccountCreditAppl(int CompanyId);
        //CustomerAccountCreditApply GetCustomerAccountCreditApplById(int CompanyId, int NetsuiteId);
        //void DeleteCustomerAccountCreditApply(CustomerAccountCreditApply credit);

        //void UpdateCustomerAccountCreditApply(CustomerAccountCreditApply credit);
        IList<CustomerAccountCredit> GetCustomerAccountCredit(int CompanyId);
        void UpdateCustomerAccountCredit(CustomerAccountCredit credit);
        CustomerAccountCredit GetCustomerAccountCreditApplById(int Id);
    }
}
