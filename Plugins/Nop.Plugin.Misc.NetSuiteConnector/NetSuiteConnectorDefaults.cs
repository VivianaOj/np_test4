namespace Nop.Plugin.Misc.NetSuiteConnector
{
    public class NetSuiteConnectorDefaults
    {
        public static string ImportCustomerTaskName => "Import Customers From NetSuite To NopCommerce";

        public static string ImportCustomerTaskType => "Nop.Plugin.Misc.NetSuiteConnector.Services.Common.ImportCustomersTask";

        public static string ImportOrderTaskName => "Import Orders From NetSuite To NopCommerce";

        public static string ImportOrderTaskType => "Nop.Plugin.Misc.NetSuiteConnector.Services.Common.ImportOrdersTask";

        public static string ImportPendingOrderTaskName => "Send Pending Orders From NopCommerce To NetSuite";

        public static string ImportPendingOrderTaskType => "Nop.Plugin.Misc.NetSuiteConnector.Services.Common.ImportPendingOrdersTask";

        public static string ImportProductTaskName => "Import Products From NetSuite To NopCommerce";

        public static string ImportProductTaskType => "Nop.Plugin.Misc.NetSuiteConnector.Services.Common.ImportProductsTask";

        public static string ImportImageTaskName => "Import Images From S3 AWS To NopCommerce";

        public static string ImportImageTaskType => "Nop.Plugin.Misc.NetSuiteConnector.Services.Common.ImportImagesTask";

        public static int TaskExecution => 3600;

        public static string ImporterControllerName => "NetSuiteImporter";

        public static string ImportProductsActionName => "ImportProducts";

        public static string ImportCustomersActionName => "ImportCustomers";

        public static string ImportOrdersActionName => "ImportOrders";

        public static string ImportPendingOrdersActionName => "ImportPendingOrders";
        public static string ImportImagesActionName => "ImportImages";

        public static string ImportProductsName => "Import Product From NetSuite To NopCommerce";

        public static string ImportCustomersName => "Import Customers From NetSuite To NopCommerce";

        public static string ImportOrdersName => "Import Order From NetSuite To NopCommerce";

        public static string ImportPendingOrdersName => "Send Pending Orders From nopCommerce to NetSuite";
        public static string ImportImagesName => "Import Images From AWS To NopCommerce";

        public static string DefaultIcon => "fa-genderless";

        public static string MainMenuSystemName => "NetSuiteConnector-Main-Menu";

        public static string ImporterMenuSystemName => "NetSuiteConnector-Importer-Menu";

        public static string ConfigurationMenuSystemName => "NetSuiteConnector-Configuration-Menu";

        public static string ImportDataNetSuiteNopCommerce => "Nop.Plugin.Misc.NetSuiteConnector.Services.Common.ImportDataCustomersTask";
        public static string ImportDataFromNetsuiteTaskName => "Import Data From NetSuite To NopCommerce";
        public static string ImportDataFromNetsuiteActionName => "ImportDataFromNetsuite";
    }
}
