using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.NetSuiteConnector.Data;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Custom
{
    public class CompanyService : ICompanyService
    {
        #region Fields

        private readonly IRepository<Company> _repository;
        private readonly IRepository<CompanyAddresses> _repositoryCompanyAddress;
        private readonly IRepository<CompanyCustomerMapping> _repositoryCompanyCustomerMapping;
        //private readonly IRepository<Address> _repositoryAddress;
        private readonly NetSuiteConnectorContext _objectContext;


        #endregion

        #region Ctor

        public CompanyService(IRepository<Company> repository, IRepository<CompanyAddresses> repositoryCompanyAddress,
            IRepository<CompanyCustomerMapping> repositoryCompany_Customer_Mapping, NetSuiteConnectorContext objectContext)
        {
            _repository = repository;
            _repositoryCompanyAddress = repositoryCompanyAddress;
            _repositoryCompanyCustomerMapping = repositoryCompany_Customer_Mapping;
            _objectContext = objectContext;


        }


        #endregion


        #region Methods
        public List<CompanyAddresses> GetCompanyAddress(int companyId)
        {
            var query = _repositoryCompanyAddress.Table;
                        query = query.Where(c => c.CompanyId== companyId);

            return query.ToList();
        }

        public void InsertCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            _repository.Insert(company);
        }

        public void InsertCompanyAddress(CompanyAddresses companyAddress)
        {
            if (companyAddress == null)
                throw new ArgumentNullException(nameof(companyAddress));

            _repositoryCompanyAddress.Insert(companyAddress);
        }

        public void InsertCompanyCustomerMap(CompanyCustomerMapping companyCustomer)
        {
            if (companyCustomer == null)
                throw new ArgumentNullException(nameof(companyCustomer));

            _repositoryCompanyCustomerMapping.Insert(companyCustomer);
        }

        #endregion
    }
}
