using Nop.Core.Data;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Invoices
{
    public partial class InvoicePaymentService : IInvoicePaymentService
    {
        #region Fields
        private readonly IRepository<InvoicePayment> _invoiceRepository;
        private readonly IEventPublisher _eventPublisher;
        #endregion

        #region Ctor

        public InvoicePaymentService(IEventPublisher eventPublisher, IRepository<InvoicePayment> invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods


        /// <summary>
        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertInvoicePayment(InvoicePayment invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(invoice));

            _invoiceRepository.Insert(invoice);

            //event notification
            _eventPublisher.EntityInserted(invoice);
        }

        public void DeleteInvoicePayments(InvoicePayment invoice)
        {
            if (invoice == null)
                throw new ArgumentNullException(nameof(InvoicePayment));

            _invoiceRepository.Delete(invoice);
        }

        public InvoicePayment GetInvoicePaymentById(int Id, int invoiceId)
        {
            if (Id == 0)
                return new InvoicePayment();

            var query = from o in _invoiceRepository.Table
                        where o.Id == Id
                        select o;

            return query.FirstOrDefault();
        }

        public IList<InvoicePayment> GetInvoicePayments(int InvoiceId)
        {
            if (InvoiceId == 0)
                return new List<InvoicePayment>();

            var query = from o in _invoiceRepository.Table
                        where o.InvoiceId == InvoiceId
                        select o;

            return query.ToList();
        }

     
        #endregion



    }
}
