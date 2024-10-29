using Nop.Core.Domain.NN;
using Nop.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public partial interface ICustomerService
    {
        void ImportCustomers(string LastExecutionDate, string idcustomer = null, string type = null);
        //CustomerDto UpdateCustomer(int Id);
        void ImportCustomers(string LastExecutionDate, string idcustomer = null, string type = null, List<PendingDataToSync> listCustomers=null);

        Task<bool> ImportCustomersAsync(Microsoft.EntityFrameworkCore.DbContext dbContext, string LastExecutionDate, string idcustomer = null, string type = null);
    }
}
