using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.NetSuiteConnector.Models
{
	public partial class CustomerModelLog : BaseNopModel
	{
		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.Fields.Customer")]
		public string Customer { get; set; }

		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.ImportDataNetsuite.")]
		public string ImportDataNetsuite { get; set; }


		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.SavedDataNopcommerce")]
		public string SavedDataNopcommerce { get; set; }


		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.UpdatedOn.")]
		public string UpdatedOn { get; set; }
	}

	public partial class CustomerModelLogSearchModel : BaseSearchModel
	{
        #region Ctor

        public CustomerModelLogSearchModel()
        {
        }

		#endregion

		#region Properties
		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.Fields.Customer")]
		public string Customer { get; set; }

		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.ImportDataNetsuite.")]
		public string ImportDataNetsuite { get; set; }


		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.SavedDataNopcommerce")]
		public string SavedDataNopcommerce { get; set; }


		[NopResourceDisplayName("Admin.NetsuiteConnector.Customers.UpdatedOn.")]
		public string UpdatedOn { get; set; }

		#endregion
	}
}
