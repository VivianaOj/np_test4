using Microsoft.EntityFrameworkCore;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Customers
{
    public partial class CompanyService : ICompanyService
    {
        #region Fields

        private readonly IRepository<Company> _companyRepository;
        private readonly IRepository<CompanyCustomerMapping> _companyCustomerMappingRepository;
        private readonly IRepository<CompanyAddresses> _companyAddressMappingRepository;

        #endregion

        #region Ctor

        public CompanyService(IRepository<Company> companyRepository,
            IRepository<CompanyCustomerMapping> companyCustomerMappingRepository,
            IRepository<CompanyAddresses> companyAddressMappingRepository)
        {
            _companyRepository = companyRepository;
            _companyCustomerMappingRepository = companyCustomerMappingRepository;
            _companyAddressMappingRepository = companyAddressMappingRepository;
        }

        #endregion

        #region Methods

        #region Companies

        public void DeleteCompany(Company company)
        {
            var companyCustomerMappings = _companyCustomerMappingRepository.Table.Where(c => c.CompanyId == company.Id);

            if (companyCustomerMappings.Any())
                _companyCustomerMappingRepository.Delete(companyCustomerMappings);

            var companyAddressMappings = _companyAddressMappingRepository.Table.Where(c => c.CompanyId == company.Id);

            if (companyAddressMappings.Any())
                _companyAddressMappingRepository.Delete(companyAddressMappings);

            _companyRepository.Delete(company);
        }

        public IPagedList<Company> GetAllCompanies(string companyName = null, string netSuiteId = null, string companyEmail = null,
            int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var query = _companyRepository.Table;

            if (!string.IsNullOrEmpty(companyName))
                query = query.Where(c => c.CompanyName.Contains(companyName)).OrderBy(r => r.CompanyName);

            if (!string.IsNullOrEmpty(netSuiteId))
                query = query.Where(c => c.NetsuiteId == netSuiteId).OrderBy(r => r.CompanyName);

            if (!string.IsNullOrEmpty(companyEmail))
                query = query.Where(c => c.Email.Contains(companyEmail) || c.EmailsForBilling.Contains(companyEmail)).OrderBy(r => r.CompanyName);

            var companies = new PagedList<Company>(query, pageIndex, pageSize);

            return companies;
        }

        public void InsertCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            _companyRepository.Insert(company);
        }

        public void UpdateCompany(Company company)
        {
            if (company == null)
                throw new ArgumentNullException(nameof(company));

            _companyRepository.Update(company);
        }

        public List<Company> GetCompanyByNetSuiteId(int netSuiteId)
        {
            var query = _companyRepository.Table.Where(q => q.NetsuiteId == netSuiteId.ToString());

            return query.ToList();
        }

        public CompanyAddresses GetCompanyByAddressId(int addressId)
        {
            if (addressId == null)
                throw new ArgumentNullException(nameof(addressId));

            var query = _companyAddressMappingRepository.Table.Where(r => r.AddressId == addressId).FirstOrDefault();

            return query;
        }

        public Company GetCompanyById(int id)
        {
            return _companyRepository.GetById(id);
        }

        #endregion

        #region Company Address Mappings

        public IQueryable<CompanyAddresses>  GetAllCompanyAddressMappings(int? addressId = null, int? companyId = null)
        {
            var query = _companyAddressMappingRepository.Table.AsNoTracking();

            if (addressId != null)
                query = query.Where(q => q.AddressId == addressId);

            if (companyId != null)
                query = query.Where(q => q.CompanyId == companyId);

            return query.Include(cam => cam.Address);
        }

        public IList<CompanyAddresses> GetAllCompanyAddressMappingsList(int? addressId = null, int? companyId = null)
        {
            var query = _companyAddressMappingRepository.Table;

            if (addressId != null)
                query = query.Where(q => q.AddressId == addressId);

            if (companyId != null)
                query = query.Where(q => q.CompanyId == companyId);

            return query.ToList();
        }

        public IList<CompanyAddresses> GetAllCompanyAddressMappingsById(int companyId)
        {
            var query = _companyAddressMappingRepository.Table;

            if (companyId != 0)
                query = query.Where(q => q.CompanyId == companyId);

            return query.ToList();
        }
        public void DeleteCompanyAddressMapping(CompanyAddresses companyAddressMapping)
        {
            if (companyAddressMapping == null)
                throw new ArgumentNullException(nameof(companyAddressMapping));

            _companyAddressMappingRepository.Delete(companyAddressMapping);
        }

        public void InsertCompanyAddressMapping(CompanyAddresses companyAddressMapping)
        {
            if (companyAddressMapping == null)
                throw new ArgumentNullException(nameof(companyAddressMapping));

            _companyAddressMappingRepository.Insert(companyAddressMapping);
        }

        public void UpdateCompanyAddressMapping(CompanyAddresses companyAddressMapping)
        {
            if (companyAddressMapping == null)
                throw new ArgumentNullException(nameof(companyAddressMapping));

            _companyAddressMappingRepository.Update(companyAddressMapping);
        }
   
        #endregion

        #region Company Customer Mappings

        public IList<CompanyCustomerMapping> GetAllCompanyCustomerMappings(int? customerId = null, int? companyId = null)
        {
            var query = _companyCustomerMappingRepository.Table;

            if (customerId != null)
                query = query.Where(q => q.CustomerId == customerId && q.Active);

            if (companyId != null)
                query = query.Where(q => q.CompanyId == companyId && q.Active);

           
            return query.ToList();
        }

        public IList<CompanyCustomerMapping> GetAllCompanyCustomerMappingsActiveInactive(int? customerId = null, int? companyId = null)
        {
            var query = _companyCustomerMappingRepository.Table;

            if (customerId != null)
                query = query.Where(q => q.CustomerId == customerId );

            if (companyId != null)
                query = query.Where(q => q.CompanyId == companyId );


            return query.ToList();
        }


        public void DeleteCompanyCustomerMapping(CompanyCustomerMapping companyCustomerMapping)
        {
            if (companyCustomerMapping == null)
                throw new ArgumentNullException(nameof(companyCustomerMapping));

            _companyCustomerMappingRepository.Delete(companyCustomerMapping);
        }

        public void InsertCompanyCustomerMapping(CompanyCustomerMapping companyCustomerMapping)
        {
            if (companyCustomerMapping == null)
                throw new ArgumentNullException(nameof(companyCustomerMapping));

            _companyCustomerMappingRepository.Insert(companyCustomerMapping);
        }

        public void UpdateCompanyCustomerMapping(CompanyCustomerMapping companyCustomerMapping)
        {
            if (companyCustomerMapping == null)
                throw new ArgumentNullException(nameof(companyCustomerMapping));

            _companyCustomerMappingRepository.Update(companyCustomerMapping);
        }


        public void SetDefaulCompanyCustomer(int companyId, int customerId)
        {
            var query = _companyCustomerMappingRepository.Table;


            if (companyId != 0 && customerId != 0)
            {
                var companyCustomer = query.Where(q => q.CustomerId == customerId).ToList();
                foreach (var item in companyCustomer)
                {
                    if (item.CompanyId == companyId)
                    {
                        item.DefaultCompany = true;
                    }
                    else
                    {
                        item.DefaultCompany = false;
                    }
                }
                if (companyCustomer.Count > 0)
                {
                    _companyCustomerMappingRepository.Update(companyCustomer);
                }
            }

        }
        public List<Company> GetCompanyChildByParentId(int ParentId)
        {
            var query = _companyRepository.Table.Where(q => q.Parent_Id == ParentId);

            return query.ToList();
        }


        #endregion

        #endregion
    }
}
