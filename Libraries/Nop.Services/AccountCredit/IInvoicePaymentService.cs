using System.Collections.Generic;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;

namespace Nop.Services.Invoices
{
    public partial interface IInvoicePaymentService
    {
        void InsertInvoicePayment(InvoicePayment invoice);

        IList<InvoicePayment> GetInvoicePayments(int CompanyId);
        InvoicePayment GetInvoicePaymentById(int CompanyId, int NetsuiteId);
        void DeleteInvoicePayments(InvoicePayment credit);
    }
}
