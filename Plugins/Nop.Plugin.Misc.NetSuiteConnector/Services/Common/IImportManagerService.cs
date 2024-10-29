using System;
using System.Collections.Generic;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public interface IImportManagerService
    {
        bool ImportCustomers(int identifier, string idcustomer = null, string type = null);

        bool ImportOrders(int identifier, string idorder = null, string type = null);
        bool ImportImages();

        NetSuiteImporter GetImporter(ImporterIdentifier identifier, string idcustomer=null, string type=null);

        void UpdateImporter(NetSuiteImporter importer);

        void CreateImporter(NetSuiteImporter importer);

        List<NetSuiteImporter> GetImporters();
    }
}