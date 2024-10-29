using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Order
{
	public partial interface ICreditManageService
	{
		void GetCustomerBalance(int companyId, string LastExecutionDateGeneral);
	}
}
