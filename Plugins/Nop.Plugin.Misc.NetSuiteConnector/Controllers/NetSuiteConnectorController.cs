using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.NetSuiteConnector.Models.Configuration;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.NetSuiteConnector.Controllers
{
    public class NetSuiteConnectorController : BasePluginController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public NetSuiteConnectorController(ISettingService settingService, INotificationService notificationService,
            IPermissionService permissionService, ILocalizationService localizationService)
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _localizationService = localizationService;
        }

        #endregion

        #region Utilities

        private void PrepareModel(ConfigurationModel model)
        {
            var netSuiteConnectorSettings = _settingService.LoadSetting<NetSuiteConnectorSettings>();

            model.Enabled = netSuiteConnectorSettings.Enabled;
            model.AccountId = netSuiteConnectorSettings.AccountId;
            model.ConsumerKey = netSuiteConnectorSettings.ConsumerKey;
            model.ConsumerSecret = netSuiteConnectorSettings.ConsumerSecret;
            model.TokenId = netSuiteConnectorSettings.TokenId;
            model.TokenSecret = netSuiteConnectorSettings.TokenSecret;
            model.CompanyUrl = netSuiteConnectorSettings.CompanyUrl;
            model.RestServicesUrl = netSuiteConnectorSettings.RestServicesUrl;


            model.AccessKeyID = netSuiteConnectorSettings.AccessKeyID;
            model.SecretAccessKey = netSuiteConnectorSettings.SecretAccessKey;
            model.S3Region = netSuiteConnectorSettings.S3Region;
            model.DefaultS3Folder = netSuiteConnectorSettings.DefaultS3Folder;
            model.ImageTypes = netSuiteConnectorSettings.ImageTypes;
            model.ImagesTempFolder = netSuiteConnectorSettings.ImagesTempFolder;

        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            var model = new ConfigurationModel();
            PrepareModel(model);

            return View("~/Plugins/Misc.NetSuiteConnector/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            var netSuiteConnectorSettings = _settingService.LoadSetting<NetSuiteConnectorSettings>();

            //save settings
            netSuiteConnectorSettings.Enabled = model.Enabled;
            netSuiteConnectorSettings.AccountId = model.AccountId;
            netSuiteConnectorSettings.ConsumerKey = model.ConsumerKey;
            netSuiteConnectorSettings.ConsumerSecret = model.ConsumerSecret;
            netSuiteConnectorSettings.TokenId = model.TokenId;
            netSuiteConnectorSettings.TokenSecret = model.TokenSecret;
            netSuiteConnectorSettings.CompanyUrl = model.CompanyUrl;
            netSuiteConnectorSettings.RestServicesUrl = model.RestServicesUrl;

            netSuiteConnectorSettings.AccessKeyID = model.AccessKeyID;
            netSuiteConnectorSettings.SecretAccessKey = model.SecretAccessKey;
            netSuiteConnectorSettings.S3Region = model.S3Region;
            netSuiteConnectorSettings.ImagesTempFolder = model.ImagesTempFolder;
            netSuiteConnectorSettings.ImageTypes = model.ImageTypes;
            netSuiteConnectorSettings.DefaultS3Folder = model.DefaultS3Folder;


            _settingService.SaveSetting(netSuiteConnectorSettings);
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}
