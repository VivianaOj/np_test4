using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Custom
{
    public interface ICompanyService
    {
        void InsertCompany(Company company);
        void InsertCompanyCustomerMap(CompanyCustomerMapping insertCustomer);
        void InsertCompanyAddress(CompanyAddresses companyAddress);
        List<CompanyAddresses> GetCompanyAddress(int companyId);
    }
}