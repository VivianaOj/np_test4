using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.Shipping;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.NN;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using System;
using System.Linq;
using Taxjar;

namespace Nop.Plugin.Tax.TaxJar
{
    public class TaxJarProvider : BasePlugin, ITaxProvider
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ILocalizationService _localizationService;
        private readonly ISettingService _settingService;
        private readonly TaxJarSettings _taxJarSettings;
        private readonly ICountryService _countryService;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ILogger _logger;
        private readonly IStaticCacheManager _cacheManager;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IShippingService _shippingService;

        private readonly IWarehouseLocationNNService _warehouseLocationNNService;
        private readonly IRepository<Warehouse> _warehouseRepository;
        private readonly IAddressService _addressService;

        #endregion

        #region Variables
        Address addressFrom = new Address();
        #endregion

        #region Ctor

        public TaxJarProvider(IWebHelper webHelper,
            ILocalizationService localizationService,
            ISettingService settingService,
            TaxJarSettings taxJarSettings,
            ICountryService countryService,
            IStateProvinceService stateProvinceService, ILogger logger, IStaticCacheManager cacheManager, 
            IWorkContext workContext, IGenericAttributeService genericAttributeService, IShippingService shippingService, IWarehouseLocationNNService warehouseLocationNNService, IRepository<Warehouse> warehouseRepository
            , IAddressService addressService)
        {
            _webHelper = webHelper;
            _localizationService = localizationService;
            _settingService = settingService;
            _taxJarSettings = taxJarSettings;
            _countryService = countryService;
            _stateProvinceService = stateProvinceService;
            _logger = logger;
            _cacheManager = cacheManager;
            _workContext = workContext;
            _genericAttributeService = genericAttributeService; 
            _shippingService = shippingService;
            _warehouseLocationNNService = warehouseLocationNNService;
            _warehouseRepository = warehouseRepository;
            _addressService = addressService;
        }

        #endregion

        public CalculateTaxResult GetTaxRate(CalculateTaxRequest calculateTaxRequest)
        {

            if (calculateTaxRequest.Address == null)
                return new CalculateTaxResult { Errors = new[] { "Address is not set" } };

            var companies = _workContext.CurrentCustomer.CompanyCustomerMappings;
            var netsuiteIdCompany = "";

            if (companies.Count > 0)
            {
                var companyDefault = companies.Where(r => r.DefaultCompany).FirstOrDefault();
                if (companies.Count()==1)
                    netsuiteIdCompany = companies.FirstOrDefault().Company?.NetsuiteId;
                else
                {
                    if (companyDefault != null)
                        netsuiteIdCompany = companyDefault.Company?.NetsuiteId;
                }
               
            }
            decimal price = decimal.Zero;
            decimal shipping = decimal.Zero;
            if (calculateTaxRequest.CheckoutProcess)
            {
                price = calculateTaxRequest.Price;
                shipping= calculateTaxRequest.ShippingValue;
            }

            string TaxRateCacheKey = "Nop.taxjar.taxrate.address-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}";
            //construct a cache key
            var cacheKey = string.Format(TaxRateCacheKey,
                calculateTaxRequest.Address.Address1,
                calculateTaxRequest.Address.City,
                calculateTaxRequest.Address.StateProvinceId ?? 0,
                calculateTaxRequest.Address.CountryId ?? 0,
                calculateTaxRequest.Address.ZipPostalCode, netsuiteIdCompany, calculateTaxRequest.Customer.Id, price, shipping);

            string freight_taxableCacheKey = "Nop.taxjar.freight_taxable.address-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}";
            //construct a cache key
            var cacheKeyFreight = string.Format(freight_taxableCacheKey,
                calculateTaxRequest.Address.Address1,
                calculateTaxRequest.Address.City,
                calculateTaxRequest.Address.StateProvinceId ?? 0,
                calculateTaxRequest.Address.CountryId ?? 0,
                calculateTaxRequest.Address.ZipPostalCode, netsuiteIdCompany, calculateTaxRequest.Customer.Id, price, shipping);

            string cacheKeyAmountCollect = "Nop.taxjar.freight_taxable.address-{0}-{1}-{2}-{3}-{4}-{5}-{6}-{7}-{8}";
            //construct a cache key
            var cacheKeyAmount = string.Format(cacheKeyAmountCollect,
                calculateTaxRequest.Address.Address1,
                calculateTaxRequest.Address.City,
                calculateTaxRequest.Address.StateProvinceId ?? 0,
                calculateTaxRequest.Address.CountryId ?? 0,
                calculateTaxRequest.Address.ZipPostalCode, netsuiteIdCompany, calculateTaxRequest.Customer.Id, price, shipping);


            //we don't use standard way _cacheManager.Get() due the need write errors to CalculateTaxResult
            if (_cacheManager.IsSet(cacheKey))
            {
                return new CalculateTaxResult { TaxRate = _cacheManager.Get<decimal>(cacheKey, () => default(decimal)), freight_taxable = _cacheManager.Get<bool>(cacheKeyFreight, () => default(bool)), amount_to_collect = _cacheManager.Get<decimal>(cacheKeyAmount, () => default(decimal)), };
            }

            var client = new TaxjarApi(_taxJarSettings.TaxJarAPIKey, new { apiUrl = "https://api.taxjar.com" });
            var clientAddress = new TaxjarApi(_taxJarSettings.TaxJarAPIKey, new { apiUrl = "https://api.taxjar.com" });
            clientAddress.apiUrl = clientAddress.apiUrl+"addresses/validate";

            CalculateTaxResult calculateTaxResult = new CalculateTaxResult
            {
                TaxRate = decimal.Zero,
                freight_taxable=false
            };

            var product = calculateTaxRequest.Product;

            addressFrom.Zip = _settingService.GetSetting("taxjar.from_zip").Value;
            addressFrom.State = _settingService.GetSetting("taxjar.from_state").Value;
            addressFrom.City = _settingService.GetSetting("taxjar.from_city").Value;
            addressFrom.Street = _settingService.GetSetting("taxjar.from_street").Value;

            var pickupPointsResponse = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddress, _workContext.CurrentCustomer, storeId: 1);
            if (pickupPointsResponse.Success)
            {
                //Default value
                var pickUpPoint = pickupPointsResponse.PickupPoints.Where(r => r.Description == _settingService.GetSetting("taxjar.from_defaultpickupcity").Value).FirstOrDefault();

                if(pickUpPoint!=null)
                {
                    addressFrom.City = pickUpPoint.City;
                    addressFrom.Zip = pickUpPoint.ZipPostalCode;
                    addressFrom.State = pickUpPoint.StateAbbreviation;
                    addressFrom.Street = pickUpPoint.Address;
                }
            }


            var StateFrom = _stateProvinceService.GetStateProvinceById(calculateTaxRequest.Address.StateProvinceId.HasValue ? calculateTaxRequest.Address.StateProvinceId.Value : 0);

            if (StateFrom != null)
            {
                var Location = _warehouseLocationNNService.GetWarehouseLocationId(StateFrom.Id);

                if (Location != null)
                {
                    var warehouse = _warehouseRepository.GetById(Location.WarehouseId);

                    if (warehouse != null)
                    {
                        var address = _addressService.GetAddressById(warehouse.AddressId);


                        addressFrom.Street = address.Address1 + " " + address.Address2;
                        addressFrom.City = address.City;
                        addressFrom.State = address.StateProvince.Abbreviation;
                        //addressFrom.Country = address.Country;
                        addressFrom.Zip = address.ZipPostalCode;

                    }

                }
            }
            try
            {
                if (calculateTaxRequest.Address!=null)
                {
                    if (calculateTaxRequest.Address.CountryId.HasValue)
                    {
                        var country = _countryService.GetCountryById(calculateTaxRequest.Address.CountryId.Value);
                        if (country != null)
                        {
                            var stateProvince = _stateProvinceService.GetStateProvinceById(calculateTaxRequest.Address.StateProvinceId.HasValue ? calculateTaxRequest.Address.StateProvinceId.Value : 0);
                           // var nexus = client.NexusRegions();
                            var calculateTax = false;

                            //client.ShowCustomer(12296);

                            if (!string.IsNullOrEmpty(netsuiteIdCompany))
                            {
                                var customers = client.ListCustomers();
                                var json = JsonConvert.SerializeObject(client);

                                try
                                {
                                    var TaxjarCustomer = client.ShowCustomer(netsuiteIdCompany);
                                    if (TaxjarCustomer.ExemptRegions.Count > 0)
                                    {
                                        foreach (var item in TaxjarCustomer.ExemptRegions)
                                        {
                                            _logger.Information("Tax Jar  Customer:" + calculateTaxRequest.Customer.NetsuitId.ToString() + " Company: " + netsuiteIdCompany + " TaxjarCustomer.ExemptRegions:" + item.State);

                                            if (item.State == stateProvince.Abbreviation)
                                            {
                                                calculateTax = true;
                                                _logger.Information("Tax Jar  Customer:" + calculateTaxRequest.Customer.NetsuitId.ToString() + " Company: " + netsuiteIdCompany + " calculateTax: false");
                                            }
                                        }
                                        if (!calculateTax)
                                        {
                                            var value = TaxForOrder(calculateTaxRequest);

                                            if (value.HasNexus)
                                            {
                                                if (product == null)
                                                {
                                                    var rates = client.RatesForLocation(calculateTaxRequest.Address.ZipPostalCode, new
                                                    {
                                                        to_country = country.TwoLetterIsoCode,
                                                        to_zip = calculateTaxRequest.Address.ZipPostalCode,
                                                        to_state = stateProvince != null ? stateProvince.Abbreviation : string.Empty,
                                                        to_city = calculateTaxRequest.Address.City,
                                                        to_street = calculateTaxRequest.Address.Address1,
                                                        shipping = calculateTaxRequest.Price,

                                                    });
                                                    calculateTaxResult.freight_taxable = value.freight_taxable;
                                                    calculateTaxResult.TaxRate = (rates.CityRate + rates.CountryRate + rates.StateRate + rates.CountyRate + rates.CombinedDistrictRate) * 100;
                                                }
                                                else
                                                {
                                                    calculateTaxResult.TaxRate = value.TaxRate * 100;
                                                    calculateTaxResult.freight_taxable = value.freight_taxable;
                                                }
                                            }
                                            else
                                            {
                                                calculateTaxResult.TaxRate = value.TaxRate * 100;
                                                calculateTaxResult.freight_taxable = value.freight_taxable;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TaxForOrderWithoutExemotionRegion(calculateTaxRequest, client, calculateTaxResult, product, country, stateProvince);

                                    }
                                }
                                catch (Exception ex)
                                {
                                    calculateTaxResult.Errors.Add(ex.Message);
                                    _logger.Information("It is not a customer create in taxjar:" + calculateTaxRequest.Customer.Parent.ToString() + " Company: " + netsuiteIdCompany+ "--" + ex.Message);
                                    TaxForOrderWithoutExemotionRegion(calculateTaxRequest, client, calculateTaxResult, product, country, stateProvince);
                                }
                            }
                            else
                            {
                                var value = TaxForOrder(calculateTaxRequest);

                                if (value.HasNexus)
                                {
                                    if (product == null)
                                    {
                                        var rates = client.RatesForLocation(calculateTaxRequest.Address.ZipPostalCode, new
                                        {
                                            to_country = country.TwoLetterIsoCode,
                                            to_zip = calculateTaxRequest.Address.ZipPostalCode,
                                            to_state = stateProvince != null ? stateProvince.Abbreviation : string.Empty,
                                            to_city = calculateTaxRequest.Address.City,
                                            to_street = calculateTaxRequest.Address.Address1,
                                            shipping = calculateTaxRequest.Price,

                                        });
                                        calculateTaxResult.freight_taxable = value.freight_taxable;
                                        calculateTaxResult.TaxRate = (rates.CityRate + rates.CountryRate + rates.StateRate + rates.CountyRate + rates.CombinedDistrictRate) * 100;
                                        calculateTaxResult.amount_to_collect = value.amount_to_collect;
                                    }
                                    else
                                    {
                                        calculateTaxResult.TaxRate = value.TaxRate;
                                        calculateTaxResult.freight_taxable = value.freight_taxable;
                                        calculateTaxResult.amount_to_collect = value.amount_to_collect;
                                    }
                                }
                                else
                                {
                                    calculateTaxResult.TaxRate = value.TaxRate;
                                    calculateTaxResult.freight_taxable = value.freight_taxable;
                                    calculateTaxResult.amount_to_collect = value.amount_to_collect;
                                }
                             }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                calculateTaxResult.Errors.Add(ex.Message);
                _logger.Information("Tax Jar Error calculateTaxResult:" + ex.Message);
            }

          
            //tax rate successfully received, so cache it
            _cacheManager.Set(cacheKey, calculateTaxResult.TaxRate, 60);
            _cacheManager.Set(cacheKeyFreight, calculateTaxResult.freight_taxable, 60);
            _cacheManager.Set(cacheKeyAmount, calculateTaxResult.amount_to_collect, 60);

            return calculateTaxResult;
        }

        private void TaxForOrderWithoutExemotionRegion(CalculateTaxRequest calculateTaxRequest, TaxjarApi client, CalculateTaxResult calculateTaxResult, Core.Domain.Catalog.Product product, Core.Domain.Directory.Country country, Core.Domain.Directory.StateProvince stateProvince)
        {
            var value = TaxForOrder(calculateTaxRequest);

            if (value.HasNexus)
            {
                if (product == null)
                {
                    var rates = client.RatesForLocation(calculateTaxRequest.Address.ZipPostalCode, new
                    {
                        to_country = country.TwoLetterIsoCode,
                        to_zip = calculateTaxRequest.Address.ZipPostalCode,
                        to_state = stateProvince != null ? stateProvince.Abbreviation : string.Empty,
                        to_city = calculateTaxRequest.Address.City,
                        to_street = calculateTaxRequest.Address.Address1,
                        shipping = calculateTaxRequest.Price,

                    });
                    calculateTaxResult.freight_taxable = value.freight_taxable;
                    calculateTaxResult.HasNexus = value.HasNexus;
                    calculateTaxResult.TaxRate = value.TaxRate;
                    calculateTaxResult.amount_to_collect = value.amount_to_collect;

                    calculateTaxResult.TaxRate = (rates.CityRate + rates.CountryRate + rates.StateRate + rates.CountyRate + rates.CombinedDistrictRate) * 100;                
                }
                else
                {
                    calculateTaxResult.freight_taxable = value.freight_taxable;
                    calculateTaxResult.HasNexus = value.HasNexus;
                    calculateTaxResult.TaxRate = value.TaxRate;
                    calculateTaxResult.amount_to_collect = value.amount_to_collect;
                }
            }
            else
            {
                calculateTaxResult.freight_taxable = value.freight_taxable;
                calculateTaxResult.HasNexus = value.HasNexus;
                calculateTaxResult.TaxRate = value.TaxRate;
                calculateTaxResult.amount_to_collect = value.amount_to_collect;
            }
        }

        public CalculateTaxResult TaxForOrder(CalculateTaxRequest calculateTaxRequest)
        {
            var client = new TaxjarApi(_taxJarSettings.TaxJarAPIKey, new { apiUrl = "https://api.taxjar.com" });

            CalculateTaxResult calculateTaxResult = new CalculateTaxResult
            {
                TaxRate = decimal.Zero,
                HasNexus = false
            };

            try
            {
                if (calculateTaxRequest.Address.CountryId.HasValue)
                {
                    var country = _countryService.GetCountryById(calculateTaxRequest.Address.CountryId.Value);

                    if (country != null)
                    {
                        var stateProvince = _stateProvinceService.GetStateProvinceById(calculateTaxRequest.Address.StateProvinceId.HasValue ? calculateTaxRequest.Address.StateProvinceId.Value : 0);

                        ValidateNNDelivery(calculateTaxRequest);

                        var tax = client.TaxForOrder(new
                        {
                            from_country = country.TwoLetterIsoCode,
                            from_zip = addressFrom.Zip,
                            from_state = addressFrom.State,
                            from_city = addressFrom.City,
                            from_street = addressFrom.Street,
                            to_country = country.TwoLetterIsoCode,
                            to_zip = calculateTaxRequest.Address.ZipPostalCode,
                            to_state = stateProvince != null ? stateProvince.Abbreviation : string.Empty,
                            to_city = calculateTaxRequest.Address.City,
                            to_street = calculateTaxRequest.Address.Address1,
                            //customer_id = calculateTaxRequest.Customer.NetsuitId,
                            amount = calculateTaxRequest.Price,
                            shipping = calculateTaxRequest.ShippingValue,
                            line_items = new[] {
                            new {
                                id = calculateTaxRequest.Product?.Id.ToString(),
                                quantity = 1, //nop make the calculation by unit
                                //product_tax_code = calculateTaxRequest.TaxCategoryId.ToString(),
                                unit_price = calculateTaxRequest.Price
                                }
                            },

                        });

                        calculateTaxResult.HasNexus = tax.HasNexus;
                        calculateTaxResult.TaxRate = tax.Rate;
                        calculateTaxResult.freight_taxable = tax.FreightTaxable;
                        calculateTaxResult.amount_to_collect = tax.AmountToCollect;

                        var MessageLogTaxJar = "Taxjar Information";

                        var serializableJson = JsonConvert.SerializeObject(tax, Formatting.Indented);
                        var TaxSend = "";
                        if (tax != null)
                        {
                            TaxSend = "Rate: " + tax.Rate
                            + " tax AmountToCollect :" + tax.AmountToCollect
                            + " Address: to_country " + country?.TwoLetterIsoCode
                            + ", to_zip: " + calculateTaxRequest?.Address?.ZipPostalCode
                            + ", to_state: " + stateProvince?.Abbreviation
                            + ", to_city: " + calculateTaxRequest.Address?.City
                            + ", to_street: " + calculateTaxRequest.Address?.Address1
                            + ", amountValue: " + calculateTaxRequest.Price
                            + ", ShippingValueSend: " + calculateTaxRequest.ShippingValue
                            + ", Product Id" + calculateTaxRequest.Product?.Id.ToString()
                            + ", from_zip: " + addressFrom.Zip
                            + ", from_state: " + addressFrom.State
                            + ", from_city: " + addressFrom.City
                            + ", from_street: " + addressFrom.Street;
                        }

                        if (!calculateTaxRequest.CheckoutProcess)
                        {
                            MessageLogTaxJar = "Log Taxjar";
                        }
                        _logger.InsertLog(LogLevel.Information, MessageLogTaxJar, "nopCommerce Request: " + TaxSend + " TaxJar  Return:" + serializableJson);
                    }
                }
            }
            catch (Exception ex)
            {
                calculateTaxResult.Errors.Add(ex.Message);
            }

            return calculateTaxResult;
        }

        private void ValidateNNDelivery(CalculateTaxRequest calculateTaxRequest)
        {
            try
            {
                _logger.Information("start ValidateNNDelivery");

                var pickupPoint = _genericAttributeService.GetAttribute<ShippingOption>(calculateTaxRequest.Customer, NopCustomerDefaults.SelectedShippingOptionAttribute, 1);

                if (pickupPoint != null && pickupPoint.LocationName!=null)
                {
                    _logger.Information("start ValidateNNDelivery LocationName" + pickupPoint.LocationName);

                    if (calculateTaxRequest.Customer.NetsuitId != 0)
                    {
                        var pickupPointsResponse = _shippingService.GetPickupPoints(_workContext.CurrentCustomer.BillingAddress, _workContext.CurrentCustomer, storeId: 1);

                        _logger.Information("start ValidateNNDelivery PickupPoints count" + pickupPointsResponse.PickupPoints.Count());

                        if (pickupPointsResponse.Success)
                        {
                            var pickUpPointNNDelivery = pickupPointsResponse.PickupPoints.Where(r => r.Description == pickupPoint.LocationName).FirstOrDefault();

                            addressFrom.City = pickUpPointNNDelivery.City;
                            addressFrom.Zip = pickUpPointNNDelivery.ZipPostalCode;
                            addressFrom.State = pickUpPointNNDelivery.StateAbbreviation;
                            addressFrom.Street = pickUpPointNNDelivery.Address;
                            _logger.Information("TaxJar Validate NNDelivery: pickupPoint.LocationName=>" + pickupPoint.LocationName);

                        }
                    }

                }
            }
            catch (Exception ex)
            {
                _logger.Information("TaxJar No possible ValidateNNDelivery :" + calculateTaxRequest.Customer.Parent.ToString()  +"--" + ex.Message);
            }
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/TaxJar/Configure";
        }

        public override void Install()
        {
            _settingService.SaveSetting(new TaxJarSettings());

            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Tax.TaxJar", "TaxJar");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Tax.TaxJar.Fields.TaxJarAPIKey", "Tax Jar API Key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Tax.TaxJar.Fields.TaxJarAPIKey.Hint", "Tax Jar API Key");

            base.Install();
        }

        /// <summary>
        /// Uninstall the plugin
        /// </summary>
        public override void Uninstall()
        {
            _settingService.DeleteSetting<TaxJarSettings>();

            _localizationService.DeletePluginLocaleResource("Plugins.Tax.TaxJar");
            _localizationService.DeletePluginLocaleResource("Plugins.Tax.TaxJar.Fields.TaxJarAPIKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Tax.TaxJar.Fields.TaxJarAPIKey.Hint");

            base.Uninstall();
        }
    }
}
