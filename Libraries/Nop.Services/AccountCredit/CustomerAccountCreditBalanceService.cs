using Nop.Core.Data;
using Nop.Core.Domain.Invoice;
using Nop.Services.AccountCredit;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Invoices
{
    public partial class CustomerAccountCreditBalanceService : ICustomerCreditBalanceService
    {
        #region Fields
        
        private readonly IRepository<CustomerAccountCredit> _creditRepository;
        private readonly IEventPublisher _Publisher;
        
        #endregion

        #region Ctor

        public CustomerAccountCreditBalanceService(IEventPublisher eventPublisher, IRepository<CustomerAccountCredit> invoiceRepository)
        {
            _creditRepository = invoiceRepository;
            _Publisher = eventPublisher;
        }

       

        #endregion

        #region Methods

        /// Inserts an order
        /// </summary>
        /// <param name="order">Order</param>
        public virtual void InsertCustomerCreditBalance(CustomerAccountCredit credit)
        {
            if (credit == null)
                throw new ArgumentNullException(nameof(credit));

            _creditRepository.Insert(credit);

            //event notification
            _Publisher.EntityInserted(credit);
        }

        public virtual void UpdateCustomerCreditBalance(CustomerAccountCredit credit)
        {
            if (credit == null)
                throw new ArgumentNullException(nameof(credit));

            _creditRepository.Update(credit);

            //event notification
            _Publisher.EntityInserted(credit);
        }

        public void DeleteCustomerCreditBalance(CustomerAccountCredit credit)
        {
            if (credit == null)
                throw new ArgumentNullException(nameof(CustomerAccountCredit));

            _creditRepository.Delete(credit);
        }

        public IList<CustomerAccountCredit> GetCustomerCreditBalance(int CompanyId)
        {
            if (CompanyId == 0)
                return new List<CustomerAccountCredit>();

            var query = from o in _creditRepository.Table
                        where o.CompanyId == CompanyId
                        select o;

            return query.ToList();
        }

        public CustomerAccountCredit GetCustomerCreditBalanceById(int CompanyId, int NetsuiteId)
        {
            if (CompanyId == 0)
                return new CustomerAccountCredit();

            var query = from o in _creditRepository.Table
                        where o.CompanyId == CompanyId && o.NetsuiteId == NetsuiteId
                        select o;

            return query.FirstOrDefault();
        }

        public void DeleteCustomerCreditBalance(int CompanyId)
        {
            var ListCredit = GetCustomerCreditBalance(CompanyId);

            foreach (var item in ListCredit)
            {
                DeleteCustomerCreditBalance(item);
            }
        }


        #endregion

    }
}
