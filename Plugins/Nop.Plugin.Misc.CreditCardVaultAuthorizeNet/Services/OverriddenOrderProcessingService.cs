using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Affiliates;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services
{
    public class OverriddenOrderProcessingService : OrderProcessingService
    {
        #region Fields

        private readonly ICreditCardVaultService _creditCardVaultService;
        private readonly IWorkContext _workContext;

        #endregion

        #region Ctor

        public OverriddenOrderProcessingService(CurrencySettings currencySettings,
            IAffiliateService affiliateService,
            ICheckoutAttributeFormatter checkoutAttributeFormatter,
            ICountryService countryService,
            ICurrencyService currencyService,
            ICustomerActivityService customerActivityService,
            ICustomerService customerService,
            ICustomNumberFormatter customNumberFormatter,
            IDiscountService discountService,
            IEncryptionService encryptionService,
            IEventPublisher eventPublisher,
            IGenericAttributeService genericAttributeService,
            IGiftCardService giftCardService,
            IHttpContextAccessor httpContextAccessor,
            ILanguageService languageService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderService orderService,
            IOrderTotalCalculationService orderTotalCalculationService,
            IPaymentPluginManager paymentPluginManager,
            IPaymentService paymentService,
            IPdfService pdfService,
            IPriceCalculationService priceCalculationService,
            IPriceFormatter priceFormatter,
            IProductAttributeFormatter productAttributeFormatter,
            IProductAttributeParser productAttributeParser,
            IProductService productService,
            IRewardPointService rewardPointService,
            IShipmentService shipmentService,
            IShippingPluginManager shippingPluginManager,
            IShippingService shippingService,
            IShoppingCartService shoppingCartService,
            IStateProvinceService stateProvinceService,
            IStoreContext storeContext,
            ITaxService taxService,
            IVendorService vendorService,
            IWebHelper webHelper,
            IWorkContext workContext,
            IWorkflowMessageService workflowMessageService,
            IAddressService addressService,
            ISettingService settingService,
            LocalizationSettings localizationSettings,
            OrderSettings orderSettings,
            PaymentSettings paymentSettings,
            RewardPointsSettings rewardPointsSettings,
            ShippingSettings shippingSettings,
            TaxSettings taxSettings,
            ICreditCardVaultService creditCardVaultService) : base(currencySettings,
                affiliateService,
                checkoutAttributeFormatter,
                countryService,
                currencyService,
                customerActivityService,
                customerService,
                customNumberFormatter,
                discountService,
                encryptionService,
                eventPublisher,
                genericAttributeService,
                giftCardService,
                languageService,
                localizationService,
                logger,
                orderService,
                orderTotalCalculationService,
                paymentPluginManager,
                paymentService,
                pdfService,
                priceCalculationService,
                priceFormatter,
                productAttributeFormatter,
                productAttributeParser,
                productService,
                rewardPointService,
                shipmentService,
                shippingPluginManager,
                shippingService,
                shoppingCartService,
                stateProvinceService,
                storeContext,
                taxService,
                vendorService,
                webHelper,
                workContext,
                workflowMessageService,
                addressService,
                settingService,
                localizationSettings,
                orderSettings,
                paymentSettings,
                rewardPointsSettings,
                shippingSettings,
                taxSettings)
        {
            _creditCardVaultService = creditCardVaultService;
            _workContext = workContext;
        }

        #endregion

        protected override Order SaveOrderDetails(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
        { 
            var order = base.SaveOrderDetails(processPaymentRequest, processPaymentResult, details);
            if (!_workContext.CurrentCustomer.IsGuest())
            {
                if (processPaymentRequest.SaveCard)
                    _creditCardVaultService.SaveCreditCard(_workContext.CurrentCustomer, processPaymentRequest);
            }
            return order;
           
        }

        protected override Order SaveCreditDetails(ProcessPaymentRequest processPaymentRequest, ProcessPaymentResult processPaymentResult, PlaceOrderContainer details)
        {
            var order = base.SaveCreditDetails(processPaymentRequest, processPaymentResult, details);
            if (!_workContext.CurrentCustomer.IsGuest())
            {
                if (processPaymentRequest.SaveCard)
                    _creditCardVaultService.SaveCreditCard(_workContext.CurrentCustomer, processPaymentRequest);
            }
            return order;

        }
    }
}
