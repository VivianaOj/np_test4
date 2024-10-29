using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Payments;
using Nop.Services.Plugins;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet
{
    public class CreditCardVaultAuthorizeNetPlugin : BasePlugin, IMiscPlugin, ICreditCardVault
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly ICreditCardVaultService _creditCardVaultService;

        #endregion

        #region Ctor

        public CreditCardVaultAuthorizeNetPlugin(IWebHelper webHelper,
            ISettingService settingService,
            ILocalizationService localizationService,
            ICreditCardVaultService creditCardVaultService)
        {
            _webHelper = webHelper;
            _settingService = settingService;
            _localizationService = localizationService;
            _creditCardVaultService = creditCardVaultService;
        }

        #endregion

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new CreditCardVaultAuthorizeNetSettings());

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.Enabled", "Enable");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.TransactionKey", "Transaction Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.LoginID", "Login ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.Enabled.Hint", "Enable the credit card store");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.TransactionKey.Hint", "Transaction Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.LoginID.Hint", "Login ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet", "Credit Card Vault (Authorize .Net)");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType", "Customer Type");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType.Hint", "Type of customer.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.ValidationMode", "Validation Mode");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.ValidationMode.Hint", "Use Test to perform a Luhn mod-10 check on the card number, without further validation. Use Live to submit a zero-dollar or one-cent transaction (depending on card type and processor support) to confirm the card number belongs to an active credit or debit account.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType", "Default Country");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType.Hint", "	Country of customer’s billing address.");

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<CreditCardVaultAuthorizeNetSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.Enabled");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.TransactionKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.LoginID");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.Enabled.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.TransactionKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.LoginID.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.ValidationMode");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.ValidationMode.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType");
            _localizationService.DeletePluginLocaleResource("Plugins.Misc.CreditCardVaultAuthorizeNet.CustomerType.Hint");

            base.Uninstall();
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/CreditCardVaultAuthorizeNet/Configure";
        }

        public string SaveCreditCard(Customer customer, ProcessPaymentRequest processPaymentRequest)
        {
            return _creditCardVaultService.SaveCreditCard(customer, processPaymentRequest);
        }

    }
}
