using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Shipping.NNDelivery.Data;
using Nop.Plugin.Shipping.NNDelivery.Domain;
using Nop.Plugin.Shipping.NNDelivery.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Plugins;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace Nop.Plugin.Shipping.NNDelivery
{
    public class NNDeliveryComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly NNDeliveryContext _objectContext;
        private readonly IDeliveryRoutesService _deliveryRoutesService;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _totalCalculationService;
        private readonly NNDeliverySettings _nnDeliverySettings;
        private readonly ILogger _loggerService;
        private readonly IWebHelper _webHelper;
        private readonly ICompanyService _companyService;

        #endregion

        #region Ctor

        public NNDeliveryComputationMethod(IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            NNDeliveryContext objectContext,
            IDeliveryRoutesService deliveryRoutesService,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService totalCalculationService,
            NNDeliverySettings nnDeliverySettings,
            ILogger loggerService,
            IWebHelper webHelper,
            ICompanyService companyService)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _objectContext = objectContext;
            _deliveryRoutesService = deliveryRoutesService;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _totalCalculationService = totalCalculationService;
            _nnDeliverySettings = nnDeliverySettings;
            _loggerService = loggerService;
            _webHelper = webHelper;
            _companyService = companyService;
        }

        #endregion

        #region Methods

        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            return _nnDeliverySettings.FixedRate;
        }

        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            if (getShippingOptionRequest == null)
                throw new ArgumentNullException(nameof(getShippingOptionRequest));

            var response = new GetShippingOptionResponse();

            if (getShippingOptionRequest.Items == null || !getShippingOptionRequest.Items.Any())
            {
                response.AddError("No shipment items");
                return response;
            }

            if (getShippingOptionRequest.ShippingAddress == null)
            {
                response.AddError("Shipping address is not set");
                return response;
            }

            try
            {
                bool shipmentAvailable = true;
                bool disableShipment = false;
                decimal diference = 0;
                string ValueToSendOrder = "";
                int ValueToSendOrderId = 0;
                string ValueToSendLocation = "";

                if (_workContext.CurrentCustomer.NetsuitId == 0)
                    shipmentAvailable = false;
               
                var companyAddressMappingSelected = _companyService.GetAllCompanyAddressMappings(getShippingOptionRequest.ShippingAddress.Id);

                if (companyAddressMappingSelected == null)
                {
                    var companyNNDeliveryApproved = companyAddressMappingSelected
                         .Where(cam => cam.DeliveryRoute && !cam.Address.Active &&
                                       cam.Address.Email == _workContext.CurrentCustomer.Email &&
                                       cam.CompanyId == _workContext.WorkingCompany.Id)
                         .ToList();

                    if (companyNNDeliveryApproved.Any())
                    {
                        foreach (var companyAddressMapping in companyNNDeliveryApproved)
                        {

                            string city = companyAddressMapping.Address.City;
                            if (!string.IsNullOrEmpty(city))
                                GetDeliveryRouteInformation(getShippingOptionRequest, response, ref shipmentAvailable, ref disableShipment, ref diference, ref ValueToSendOrder, ref ValueToSendOrderId, ref ValueToSendLocation, companyAddressMapping, city);
                        }
                    }
                }
				else
				{
                    string city = getShippingOptionRequest.ShippingAddress.City;
                    GetDeliveryRouteInformation(getShippingOptionRequest, response, ref shipmentAvailable, ref disableShipment, ref diference, ref ValueToSendOrder, ref ValueToSendOrderId, ref ValueToSendLocation, companyAddressMappingSelected.FirstOrDefault(cam => cam.DeliveryRoute), city);
                }


                /*
                var companyAddressMapping = _companyService.GetAllCompanyAddressMappings(getShippingOptionRequest.ShippingAddress.Id)
                    .FirstOrDefault(cam =>cam.DeliveryRoute);

                if (companyAddressMapping == null)
                    shipmentAvailable = false;

                if(getShippingOptionRequest.Customer.Parent==0)
                    shipmentAvailable = false;

                var city = getShippingOptionRequest.ShippingAddress.City;

                if (companyAddressMapping != null)
                {
                    if (companyAddressMapping.DeliveryRouteName != null)
                        city = companyAddressMapping.DeliveryRouteName;
                }

                var deliveryRoute = _deliveryRoutesService.GetByLocation(city.ToLower());

                if (deliveryRoute == null)
                    shipmentAvailable = false;

                var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

                _totalCalculationService.GetShoppingCartSubTotal(cart, true, out _, out _, out _, out var subtotal,0);


                if (deliveryRoute != null)
                {
                    ValueToSendOrder = deliveryRoute.ValueToSend;
                    ValueToSendOrderId = deliveryRoute.IdValueToSend;
                    ValueToSendLocation = deliveryRoute.Location;
                }
                

                if (deliveryRoute != null && shipmentAvailable)
                {
                    if (deliveryRoute.Minimum > subtotal + 10)
                    {
                        disableShipment = true;
                        shipmentAvailable = false;
                        diference = deliveryRoute.Minimum - Math.Round(subtotal, 2)  ;

                    }
                }
                    
                if (shipmentAvailable)
                {
                    response.ShippingOptions.Add(new Core.Domain.Shipping.ShippingOption
                    {
                        Description = string.Empty,
                        Name = _localizationService.GetResource("Nop.Plugin.Shipping.NNDelivery.Name"),
                        Rate = _nnDeliverySettings.FixedRate,
                        ShippingRateComputationMethodSystemName = "Shipping.NNDelivery",
                        ValueToSendOrder = ValueToSendOrder,
                        IdValueToSendOrder = ValueToSendOrderId,
                        LocationName = ValueToSendLocation,
                        Disable = false
                    }) ;
                }

                if (disableShipment)
                {
                    response.ShippingOptions.Add(new Core.Domain.Shipping.ShippingOption
                    {
                        Description = "Add" + " $"+ diference +" "+ "more for $" + _nnDeliverySettings.FixedRate + " N&N delivery",
                        Name = _localizationService.GetResource("Nop.Plugin.Shipping.NNDelivery.Name"),
                        Rate = _nnDeliverySettings.FixedRate,
                        ShippingRateComputationMethodSystemName = "Shipping.NNDelivery",
                        ValueToSendOrder = ValueToSendOrder,
                        IdValueToSendOrder = ValueToSendOrderId,
                        LocationName = ValueToSendLocation,
                        Disable = true
                    }) ;
                }*/
            }
            catch (Exception ex)
            {
                _loggerService.InsertLog(Core.Domain.Logging.LogLevel.Error, ex.Message, ex.StackTrace, _workContext.CurrentCustomer);
            }

            return response;
        }

		private void GetDeliveryRouteInformation(GetShippingOptionRequest getShippingOptionRequest, GetShippingOptionResponse response, ref bool shipmentAvailable, ref bool disableShipment, ref decimal diference, ref string ValueToSendOrder, ref int ValueToSendOrderId, ref string ValueToSendLocation, Core.Domain.Customers.CompanyAddresses companyAddressMapping, string City)
		{
            shipmentAvailable = true;
            if (companyAddressMapping == null)
				shipmentAvailable = false;

			if (getShippingOptionRequest.Customer.Parent == 0)
				shipmentAvailable = false;

            var city = City; 

			if (companyAddressMapping != null)
			{
				if (companyAddressMapping.DeliveryRouteName != null)
					city = companyAddressMapping.DeliveryRouteName;
			}

			var deliveryRoute = _deliveryRoutesService.GetByLocation(city.ToLower());

			if (deliveryRoute == null)
				shipmentAvailable = false;

			var cart = _shoppingCartService.GetShoppingCart(_workContext.CurrentCustomer, ShoppingCartType.ShoppingCart, _storeContext.CurrentStore.Id);

			_totalCalculationService.GetShoppingCartSubTotal(cart, true, out _, out _, out _, out var subtotal, 0);


            //var minimumsAmountForRoutes = companyAddressMapping.Company.minimumsAmountForRoutes;

            if (deliveryRoute != null)
			{
				ValueToSendOrder = deliveryRoute.ValueToSend;
				ValueToSendOrderId = deliveryRoute.IdValueToSend;
				ValueToSendLocation = deliveryRoute.Location;
			}


            if (deliveryRoute != null && shipmentAvailable)
            {
                if (deliveryRoute.Minimum > subtotal + 10)
                {
                    disableShipment = true;
                    shipmentAvailable = false;
                    diference = deliveryRoute.Minimum - Math.Round(subtotal, 2);

                }
                else
                {
                    disableShipment = false;
                }
            }


            if (shipmentAvailable)
			{
				response.ShippingOptions.Add(new Core.Domain.Shipping.ShippingOption
				{
                    Address = companyAddressMapping.Address.Address1 + ", " + companyAddressMapping.Address.ZipPostalCode,
                    AddressId = companyAddressMapping.Address.Id,
                    Description = string.Empty,
					Name = _localizationService.GetResource("Nop.Plugin.Shipping.NNDelivery.Name"),
					Rate = _nnDeliverySettings.FixedRate,
					ShippingRateComputationMethodSystemName = "Shipping.NNDelivery",
					ValueToSendOrder = ValueToSendOrder,
					IdValueToSendOrder = ValueToSendOrderId,
					LocationName = ValueToSendLocation,
					Disable = false
				});
			}

			if (disableShipment)
			{
				response.ShippingOptions.Add(new Core.Domain.Shipping.ShippingOption
				{
                    Address= companyAddressMapping.Address.Address1 +", "+ companyAddressMapping.Address.ZipPostalCode,
                    AddressId = companyAddressMapping.Address.Id,
                    Description = "Add" + " $" + diference + " " + "to meet minimum",
					Name = _localizationService.GetResource("Nop.Plugin.Shipping.NNDelivery.Name"),
					Rate = _nnDeliverySettings.FixedRate,
					ShippingRateComputationMethodSystemName = "Shipping.NNDelivery",
					ValueToSendOrder = ValueToSendOrder,
					IdValueToSendOrder = ValueToSendOrderId,
					LocationName = ValueToSendLocation,
					Disable = true
				});
			}
		}

		#endregion

		#region Utilities

		public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/NNDelivery/Configure";
        }

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new NNDeliverySettings
            {
                FixedRate = decimal.Zero
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.FixedRate", "Fixed Rate");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.FixedRate.Hint", "Fixed Rate");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.DeliveryRoutes", "Delivery Routes");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Description", "Use local N&N Delivery");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.name", "N&N Delivery");

            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Name", "Name");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Location", "Location");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Available", "Available");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Minimum", "Minimum");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Name.Hint", "Name");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Location.Hint", "Location");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Available.Hint", "Available");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Minimum.Hint", "Minimum");

            //database objects
            _objectContext.Install();

            #region Delivery Routes

            var deliveryRoute = new DeliveryRoutes
            {
                Location = "Atlanta",
                Name = "Pensacola",
                ValueToSend = "1_Pensacola",
                IdValueToSend =2,
                Available = true,
                Minimum = 950
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Atlanta",
                Name = "Carolinas",
                ValueToSend = "2_Carolinas",
                IdValueToSend = 3,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Atlanta",
                Name = "Birmingham",
                ValueToSend = "3_Birmingham",
                IdValueToSend = 4,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Atlanta",
                Name = "Columbus",
                ValueToSend = "4_Columbus",
                IdValueToSend = 5,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Atlanta",
                Name = "Macon",
                ValueToSend = "5_Macon",
                IdValueToSend = 6,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Atlanta",
                Name = "Charlotte",
                ValueToSend = "",
                IdValueToSend = 0,
                Available = true,
                Minimum = 950
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Atlanta",
                Name = "Atlanta",
                ValueToSend = "Local",
                IdValueToSend = 1,
                Available = true,
                Minimum = 500
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Canton",
                ValueToSend = "1_Canton/Columbus",
                IdValueToSend = 2,
                Available = true,
                Minimum = 1000
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Akron",
                ValueToSend = "",
                IdValueToSend = 0,
                Available = true,
                Minimum = 1000
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Columbus",
                ValueToSend = "1_Canton/Columbus",
                IdValueToSend = 2,
                Available = true,
                Minimum = 500
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Dayton",
                ValueToSend = "2_Dayton",
                IdValueToSend = 3,
                Available = true,
                Minimum = 500
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Cleveland",
                ValueToSend = "3_Cleveland/Columbus",
                IdValueToSend = 4,
                Available = true,
                Minimum = 1000
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Indy",
                ValueToSend = "4_Indy",
                IdValueToSend = 5,
                Available = true,
                Minimum = 500
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Lexington",
                ValueToSend = "5_Lexington",
                IdValueToSend = 6,
                Available = true,
                Minimum = 500
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Cincinnati",
                Name = "Cincinnati",
                ValueToSend = "Local",
                IdValueToSend = 1,
                Available = true,
                Minimum = 500
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Nashville",
                Name = "Evansville",
                ValueToSend = "1_Evansville",
                IdValueToSend = 2,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Nashville",
                Name = "Louisville",
                ValueToSend = "2_Louisville",
                IdValueToSend = 3,
                Available = true,
                Minimum = 1250
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Nashville",
                Name = "Huntsville",
                ValueToSend = "3_Huntsville",
                IdValueToSend = 4,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Nashville",
                Name = "Memphis",
                ValueToSend = "4_Memphis",
                IdValueToSend = 5,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Nashville",
                Name = "Chattanooga",
                ValueToSend = "5_Chattanooga",
                IdValueToSend = 6,
                Available = true,
                Minimum = 750
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            deliveryRoute = new DeliveryRoutes
            {
                Location = "Nashville",
                Name = "Nashville",
                ValueToSend = "Local",
                IdValueToSend = 1,
                Available = true,
                Minimum = 500
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoute);

            #endregion

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<NNDeliverySettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.FixedRate");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.FixedRate.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.DeliveryRoutes");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Description");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.name");

            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Name");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Location");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Available");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Minimum");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Name.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Location.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Available.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNDelivery.Fields.Minimum.Hint");

            //database objects
            _objectContext.Uninstall();

            base.Uninstall();
        }

        public List<Package> GetShippingPackingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            throw new NotImplementedException();
        }

		public GetShippingOptionRequest GetBoxesPackingData(GetShippingOptionRequest shippingOptionRequest)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Properties

		public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Offline;

        public IShipmentTracker ShipmentTracker => null;

        #endregion
    }
}