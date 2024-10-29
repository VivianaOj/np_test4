using Nop.Core.Domain.Orders;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Nop.Services.NNServices
{
	public partial interface INetsuiteConnectorService : IPlugin
	{
		Task<bool> ImportCustomers(string customerId, string type);

	}
}
