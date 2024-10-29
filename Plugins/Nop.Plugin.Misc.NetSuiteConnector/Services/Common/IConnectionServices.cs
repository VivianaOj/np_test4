using System.Threading.Tasks;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public interface IConnectionServices
    {
        string GetConnection(string record, string method, string bodyContent = null);
       Task<string> GetConnectionAsync(string record, string method, string bodyContent = null);
    }
}