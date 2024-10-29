using System;
using System.Collections.Generic;
using Nop.Core.Domain.NN;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.Misc.NetSuiteConnector.Models.NetSuiteImporter
{
    public class NetSuiteImporterModel : BaseNopEntityModel
    {
        public NetSuiteImporterModel()
        {
            ProductLogs = new List<LogsListByImportDate>();
            CustomerLogs = new List<LogNetsuiteImport>();
            OrderLogs = new List<LogsListByImportDate>();
            PendingOrdersLogs = new List<LogsListByImportDate>();
            ImagesLogs = new List<LogsListByImportDate>();
            CustomerModel = new CustomerModel();
            CustomerLogsIssue = new List<LogsListByImportDate>();
        }
        public string Name { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public DateTime? LastExecutionDate { get; set; }

        public string ActiveAll { get; set; }

        public List<LogsListByImportDate> ProductLogs { get; set; }
        public List<LogNetsuiteImport> CustomerLogs { get; set; }
        public List<LogsListByImportDate> CustomerLogsIssue { get; set; }

        public CustomerModel CustomerModel { get; set; }
        public List<LogsListByImportDate> OrderLogs { get; set; }
        public List<LogsListByImportDate> PendingOrdersLogs { get; set; }
        public List<LogsListByImportDate> ImagesLogs { get; set; }

        public List<PendingDataToSync> ApiLogs { get; set; }


    }
    public class LogsImportModel
    {
        public string Name { get; set; }
        public string LastExecutionDate { get; set; }
        public int Link { get; set; }
    }
    public class LogsListByImportDate
    {
        public string LastExecutionDate { get; set; }
        public List<LogsImportModel> Logs { get; set; }

       public string Customer { get; set; }

        public string ImportDataNetsuite { get; set; }


        public string SavedDataNopcommerce { get; set; }

        public string UpdatedOn { get; set; }
    }

}
