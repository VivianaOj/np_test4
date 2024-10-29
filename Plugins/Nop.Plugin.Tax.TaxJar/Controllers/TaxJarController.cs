using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Tax.TaxJar.Models;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Tax.TaxJar.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin]
    [AdminAntiForgery]
    public class TaxJarController : BasePluginController
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly TaxJarSettings _taxJarSettings;
        private readonly ISettingService _settingService;

        #endregion

        #region Ctor

        public TaxJarController(INotificationService notificationService,
            IPermissionService permissionService,
            ILocalizationService localizationService,
            TaxJarSettings taxJarSettings,
            ISettingService settingService)
        {
            _notificationService = notificationService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _taxJarSettings = taxJarSettings;
            _settingService = settingService;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            //prepare common properties
            var model = new ConfigurationModel
            {
                TaxJarAPIKey = _taxJarSettings.TaxJarAPIKey
            };

            return View("~/Plugins/Tax.TaxJar/Views/Configuration/Configure.cshtml", model);
        }

        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageTaxSettings))
                return AccessDeniedView();

            _taxJarSettings.TaxJarAPIKey = model.TaxJarAPIKey;

            _settingService.SaveSetting(_taxJarSettings);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}