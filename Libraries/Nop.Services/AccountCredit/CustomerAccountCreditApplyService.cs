using Nop.Core.Data;
using Nop.Core.Domain.Invoice;
using Nop.Core.Domain.Invoices;
using Nop.Services.Events;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Invoices
{
    public partial class CustomerAccountCreditApplyService : ICustomerAccountCreditApplyService
    {
        #region Fields
        private readonly IRepository<CustomerAccountCredit> _creditApplyRepository;

        #endregion

        #region Ctor

        public CustomerAccountCreditApplyService(IRepository<CustomerAccountCredit> creditApplyRepository)
        {
            _creditApplyRepository = creditApplyRepository;
        }

       

        #endregion

        #region Methods
        public CustomerAccountCredit GetCustomerAccountCreditApplById( int Id)
        {
           
            var query = from o in _creditApplyRepository.Table
                        where o.Id == Id
                        select o;

            return query.FirstOrDefault();
        }


        /// <summary>
        /// Updates the address
        /// </summary>
        /// <param name="address">Address</param>
       
        public virtual void UpdateCustomerAccountCredit(CustomerAccountCredit credit)
        {
            if (credit == null)
                throw new ArgumentNullException(nameof(credit));

            _creditApplyRepository.Update(credit);

        }

        public IList<CustomerAccountCredit> GetCustomerAccountCredit(int CompanyId)
        {

            if (CompanyId == 0)
                return new List<CustomerAccountCredit>();

            var query = from o in _creditApplyRepository.Table
                        where o.CompanyId == CompanyId  && o.IsInActive==false
                        select o;

            return query.ToList();
        }

        #endregion



    }
}
