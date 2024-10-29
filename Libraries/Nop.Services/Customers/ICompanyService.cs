using Nop.Core;
using Nop.Core.Domain.Customers;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Services.Customers
{
    public partial interface ICompanyService
    {
        #region Companies

        void DeleteCompany(Company company);

        IPagedList<Company> GetAllCompanies(string companyName = null, string netSuiteId = null, string companyEmail = null,
            int pageIndex = 0, int pageSize = int.MaxValue);

        void InsertCompany(Company company);

        void UpdateCompany(Company company);

        List<Company> GetCompanyByNetSuiteId(int netSuiteId);
        CompanyAddresses GetCompanyByAddressId(int addressId);
        Company GetCompanyById(int id);

        #endregion

        #region Company Address Mappings

        IQueryable<CompanyAddresses> GetAllCompanyAddressMappings(int? addressId = null, int? companyId = null);
        IList<CompanyAddresses> GetAllCompanyAddressMappingsList(int? addressId = null, int? companyId = null);
        void DeleteCompanyAddressMapping(CompanyAddresses companyAddressMapping);

        void InsertCompanyAddressMapping(CompanyAddresses companyAddressMapping);

        IList<CompanyAddresses> GetAllCompanyAddressMappingsById(int companyId);

        void UpdateCompanyAddressMapping(CompanyAddresses companyAddressMapping);
        #endregion

        #region Company Customer Mappings

        IList<CompanyCustomerMapping> GetAllCompanyCustomerMappings(int? customerId = null, int? companyId = null);

        void DeleteCompanyCustomerMapping(CompanyCustomerMapping companyCustomerMapping);

        void InsertCompanyCustomerMapping(CompanyCustomerMapping companyCustomerMapping);

        void UpdateCompanyCustomerMapping(CompanyCustomerMapping companyCustomerMapping);
        void SetDefaulCompanyCustomer(int companyId, int customerId);

        List<Company> GetCompanyChildByParentId(int ParentId);
        IList<CompanyCustomerMapping> GetAllCompanyCustomerMappingsActiveInactive(int? customerId = null, int? companyId = null);
        #endregion
    }
}
