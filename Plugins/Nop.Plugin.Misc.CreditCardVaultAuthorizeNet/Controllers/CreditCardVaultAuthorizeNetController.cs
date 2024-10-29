using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Controllers
{
    public class CreditCardVaultAuthorizeNetController : BasePluginController
    {
        #region Fields

        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ILocalizationService _localizationService;
        private readonly ICountryService _countryService;

        #endregion

        #region Ctor

        public CreditCardVaultAuthorizeNetController(ISettingService settingService,
            INotificationService notificationService,
            IPermissionService permissionService, 
            ILocalizationService localizationService,
            ICountryService countryService)
        {
            _settingService = settingService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _localizationService = localizationService;
            _countryService = countryService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            var model = new ConfigurationModel();
            var creditCardVaultAuthorizeNetSettings = _settingService.LoadSetting<CreditCardVaultAuthorizeNetSettings>();

            model.Enabled = creditCardVaultAuthorizeNetSettings.Enabled;
            model.TransactionKey = creditCardVaultAuthorizeNetSettings.TransactionKey;
            model.LoginId = creditCardVaultAuthorizeNetSettings.LoginId;
            model.CustomerType = creditCardVaultAuthorizeNetSettings.CustomerType;
            model.ValidationMode = creditCardVaultAuthorizeNetSettings.ValidationMode;
            model.DefaultCountry = creditCardVaultAuthorizeNetSettings.DefaultCountry;

            var countries = _countryService.GetAllCountries();

            foreach (var country in countries)
            {
                model.Countries.Add(new SelectListItem { 
                    Text = country.Name,
                    Value = country.TwoLetterIsoCode,
                    Selected = country.TwoLetterIsoCode == creditCardVaultAuthorizeNetSettings.DefaultCountry
                });
            }

            model.ValidationModes.Add(new SelectListItem
            {
                Text = "Test",
                Value = "testMode",
                Selected = "testMode" == creditCardVaultAuthorizeNetSettings.ValidationMode
            });

            model.ValidationModes.Add(new SelectListItem
            {
                Text = "Live",
                Value = "liveMode",
                Selected = "liveMode" == creditCardVaultAuthorizeNetSettings.ValidationMode
            });

            model.CustomerTypes.Add(new SelectListItem
            {
                Text = "Individual",
                Value = "individual",
                Selected = "individual" == creditCardVaultAuthorizeNetSettings.CustomerType
            });

            model.CustomerTypes.Add(new SelectListItem
            {
                Text = "Business",
                Value = "business",
                Selected = "business" == creditCardVaultAuthorizeNetSettings.CustomerType
            });

            return View("~/Plugins/Misc.CreditCardVaultAuthorizeNet/Views/Configure.cshtml", model);
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

            var creditCardVaultAuthorizeNetSettings = _settingService.LoadSetting<CreditCardVaultAuthorizeNetSettings>();

            //save settings
            creditCardVaultAuthorizeNetSettings.Enabled = model.Enabled;
            creditCardVaultAuthorizeNetSettings.TransactionKey = model.TransactionKey;
            creditCardVaultAuthorizeNetSettings.LoginId = model.LoginId;
            creditCardVaultAuthorizeNetSettings.CustomerType = model.CustomerType;
            creditCardVaultAuthorizeNetSettings.ValidationMode = model.ValidationMode;
            creditCardVaultAuthorizeNetSettings.DefaultCountry = model.DefaultCountry;

            _settingService.SaveSetting(creditCardVaultAuthorizeNetSettings);
            _settingService.ClearCache();

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        #endregion
    }
}
