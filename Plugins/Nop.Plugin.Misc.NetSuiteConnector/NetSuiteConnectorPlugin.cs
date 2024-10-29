using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.NetSuiteConnector.Data;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using Nop.Plugin.Misc.NetSuiteConnector.Services;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Custom;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.NNServices;
using Nop.Services.Plugins;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.NetSuiteConnector
{
    public class NetSuiteConnectorPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin/*, INetsuiteConnectorService*/
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly NetSuiteConnectorContext _objectContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IImportManagerService _importManagerService;
      //  private readonly ICustomerService _customerService;



        #endregion

        #region Ctor

        public NetSuiteConnectorPlugin(ISettingService settingService, ILocalizationService localizationService,
            IWebHelper webHelper, IWorkContext workContext, NetSuiteConnectorContext objectContext, 
            IScheduleTaskService scheduleTaskService, IImportManagerService importManagerService/*, ICustomerService customerService */)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _workContext = workContext;
            _objectContext = objectContext;
            _scheduleTaskService = scheduleTaskService;
            _importManagerService = importManagerService;
            //_customerService = customerService;
        }

        #endregion

        #region Methods

        public override void Install()
        {
            _settingService.SaveSetting(new NetSuiteConnectorSettings
            {
                ImageTypes = "*.jpg|*.jpeg|*.png"
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.PluginName", "NetSuite Connector");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Configuration", "Configuration");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.Enabled", "Enabled");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccountId", "Account Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerKey", "Consumer Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerSecret", "Consumer Secret");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenId", "Token Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenSecret", "Token Secret");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.CompanyUrl", "Company Url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.RestServicesUrl", "Rest Services Url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.ConnectorConfig", "NetSuite Connection Setup");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Products", "Products were synchronized");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Customers", "Customers were synchronized");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Orders", "Orders were synchronized");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.PendingOrder", "Pending orders were synchronized");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Images", "Images were synchronized");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Name", "Importer Name");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.LastExecutionDate", "Last Execution Date");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.ImportNow", "Import Now");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Importers", "NetSuite Importers");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.Enabled.Hint", "Enabled");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccountId.Hint", "Account Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerKey.Hint", "Consumer Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerSecret.Hint", "Consumer Secret");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenId.Hint", "Token Id");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenSecret.Hint", "Token Secret");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.CompanyUrl.Hint", "Company Url");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.RestServicesUrl.Hint", "Rest Services Url");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccessKeyID.Hint", "Access key fro AWS S3 service");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.SecretAccessKey.Hint", "Secret access key fro AWS S3 service");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.S3Region.Hint", "AWS S3 region");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.DefaultS3Folder.Hint", "Folder into AWS S3 where images are stored");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImagesTempFolder.Hint", "Folder into the server where images are stored temporarily during the importing process");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImageTypes.Hint", "Image extensions that will be downloaded from S3 e.g. .jpg|*.jpeg|*.png");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccessKeyID", "Access Key ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.SecretAccessKey", "Secret Access Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.S3Region", "S3Region");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.DefaultS3Folder", "S3 source folder");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImagesTempFolder", "Local temp folder");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImageTypes", "Image extensions");

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Connection", "Connection");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Media", "Media");

            //database objects
            _objectContext.Install();

            //create schedule task
            if (_scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportCustomerTaskType) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = NetSuiteConnectorDefaults.TaskExecution,
                    Name = NetSuiteConnectorDefaults.ImportCustomersName,
                    Type = NetSuiteConnectorDefaults.ImportCustomerTaskType,
                });
            }

            if (_scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportOrderTaskType) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = NetSuiteConnectorDefaults.TaskExecution,
                    Name = NetSuiteConnectorDefaults.ImportOrderTaskName,
                    Type = NetSuiteConnectorDefaults.ImportOrderTaskType,
                });
            }

            if (_scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportPendingOrderTaskType) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = NetSuiteConnectorDefaults.TaskExecution,
                    Name = NetSuiteConnectorDefaults.ImportPendingOrderTaskName,
                    Type = NetSuiteConnectorDefaults.ImportPendingOrderTaskType,
                });
            }

            if (_scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportProductTaskType) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = NetSuiteConnectorDefaults.TaskExecution,
                    Name = NetSuiteConnectorDefaults.ImportProductTaskName,
                    Type = NetSuiteConnectorDefaults.ImportProductTaskType,
                });
            }


            if (_scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportImageTaskType) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = NetSuiteConnectorDefaults.TaskExecution,
                    Name = NetSuiteConnectorDefaults.ImportImageTaskName,
                    Type = NetSuiteConnectorDefaults.ImportImageTaskType,
                });
            }

            if (_scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportDataNetSuiteNopCommerce) == null)
            {
                _scheduleTaskService.InsertTask(new ScheduleTask
                {
                    Enabled = false,
                    Seconds = NetSuiteConnectorDefaults.TaskExecution,
                    Name = NetSuiteConnectorDefaults.ImportDataFromNetsuiteTaskName,
                    Type = NetSuiteConnectorDefaults.ImportDataNetSuiteNopCommerce,
                });
            }

            //create importers
            if (!_importManagerService.GetImporters().Any())
            {
                _importManagerService.CreateImporter(new NetSuiteImporter
                {
                    ActionName = NetSuiteConnectorDefaults.ImportProductsActionName,
                    ControllerName = NetSuiteConnectorDefaults.ImporterControllerName,
                    Name = NetSuiteConnectorDefaults.ImportProductsName
                });

                _importManagerService.CreateImporter(new NetSuiteImporter
                {
                    ActionName = NetSuiteConnectorDefaults.ImportCustomersActionName,
                    ControllerName = NetSuiteConnectorDefaults.ImporterControllerName,
                    Name = NetSuiteConnectorDefaults.ImportCustomersName
                });

                _importManagerService.CreateImporter(new NetSuiteImporter
                {
                    ActionName = NetSuiteConnectorDefaults.ImportOrdersActionName,
                    ControllerName = NetSuiteConnectorDefaults.ImporterControllerName,
                    Name = NetSuiteConnectorDefaults.ImportOrdersName
                });

                _importManagerService.CreateImporter(new NetSuiteImporter
                {
                    ActionName = NetSuiteConnectorDefaults.ImportPendingOrdersActionName,
                    ControllerName = NetSuiteConnectorDefaults.ImporterControllerName,
                    Name = NetSuiteConnectorDefaults.ImportPendingOrdersName
                });

                _importManagerService.CreateImporter(new NetSuiteImporter
                {
                    ActionName = NetSuiteConnectorDefaults.ImportImagesActionName,
                    ControllerName = NetSuiteConnectorDefaults.ImporterControllerName,
                    Name = NetSuiteConnectorDefaults.ImportImagesName
                });

                _importManagerService.CreateImporter(new NetSuiteImporter
                {
                    ActionName = NetSuiteConnectorDefaults.ImportDataFromNetsuiteActionName,
                    ControllerName = NetSuiteConnectorDefaults.ImporterControllerName,
                    Name = NetSuiteConnectorDefaults.ImportDataFromNetsuiteTaskName
                });
            }
            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<NetSuiteConnectorSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.PluginName");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Configuration");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.Enabled");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccountId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerSecret");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenId");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenSecret");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.CompanyUrl");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.RestServicesUrl");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.ConnectorConfig");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Products");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Customers");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Orders");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.PendingOrder");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Name");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.LastExecutionDate");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.ImportNow");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Importers");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.Enabled.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccountId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ConsumerSecret.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.TokenSecret.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.CompanyUrl.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.RestServicesUrl.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Importer.Images");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccessKeyID.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.SecretAccessKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.S3Region.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.DefaultS3Folder.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImagesTempFolder.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImageTypes.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.AccessKeyID");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.SecretAccessKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.S3Region");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.DefaultS3Folder");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImagesTempFolder");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Fields.ImageTypes");

            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Connection");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.NetSuiteConnector.Media");

            //remove task
            var importTask = _scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportCustomerTaskType);

            if (importTask != null)
                _scheduleTaskService.DeleteTask(importTask);

            var importTaskOrder = _scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportOrderTaskType);

            if (importTaskOrder != null)
                _scheduleTaskService.DeleteTask(importTaskOrder);

            var importTaskPendingOrder = _scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportPendingOrderTaskType);

            if (importTaskPendingOrder != null)
                _scheduleTaskService.DeleteTask(importTaskPendingOrder);

            var importTaskProduct = _scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportProductTaskType);

            if (importTaskProduct != null)
                _scheduleTaskService.DeleteTask(importTaskProduct);

            var importTaskImage = _scheduleTaskService.GetTaskByType(NetSuiteConnectorDefaults.ImportImageTaskType);

            if (importTaskImage != null)
                _scheduleTaskService.DeleteTask(importTaskImage);

            base.Uninstall();
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/NetSuiteConnector/Configure";
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var pluginMainMenu = new SiteMapNode
            {
                Title = _localizationService.GetResource("Plugins.Misc.NetSuiteConnector.PluginName", languageId: _workContext.WorkingLanguage.Id, defaultValue: "NetSuite Connector"),
                Visible = false,
                SystemName = NetSuiteConnectorDefaults.MainMenuSystemName,
                IconClass = NetSuiteConnectorDefaults.DefaultIcon
            };

            pluginMainMenu.ChildNodes.Add(new SiteMapNode
            {
                Title = _localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Configuration", languageId: _workContext.WorkingLanguage.Id, defaultValue: "Configuration"),
                Url = _webHelper.GetStoreLocation() + "Admin/NetSuiteConnector/Configure",
                Visible = false,
                SystemName = NetSuiteConnectorDefaults.ConfigurationMenuSystemName,
                IconClass = NetSuiteConnectorDefaults.DefaultIcon
            });

            pluginMainMenu.ChildNodes.Add(new SiteMapNode
            {
                Title = _localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Importer.Importers", languageId: _workContext.WorkingLanguage.Id, defaultValue: "NetSuite Importers"),
                Url = _webHelper.GetStoreLocation() + "Admin/" + NetSuiteConnectorDefaults.ImporterControllerName,
                Visible = false,
                SystemName = NetSuiteConnectorDefaults.ImporterMenuSystemName,
                IconClass = NetSuiteConnectorDefaults.DefaultIcon
            });

            rootNode.ChildNodes.Add(pluginMainMenu);
        }

        #endregion

        //public async Task<bool> ImportCustomers(string customerId, string type)
        //{
        //     return await _customerService.ImportCustomersAsync(null, customerId,  type).ConfigureAwait(false);
        //}
    }
}
