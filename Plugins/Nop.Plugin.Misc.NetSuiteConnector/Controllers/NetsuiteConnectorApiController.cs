using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Nop.Core.Infrastructure;
using Nop.Services.NN;
using Nop.Core.Domain.NN;
using System.Threading.Tasks;
using System.Collections.Generic;
using Nop.Services.Shipping;
using Nop.Core.Domain.Common;
using System.Linq;
using Nop.Services.Directory;
using static Nop.Services.Shipping.GetShippingOptionRequest;
using Nop.Services.Catalog;

namespace Nop.Plugin.Misc.NetSuiteConnector.Controllers
{
        [Route("api/netsuite-connector")]
        [ApiController]

        public class NetsuiteConnectorApiController : ControllerBase
        {
        private readonly ISettingService _settingService;
        private  ILogger _logger;
        private IPendingDataToSyncService _pendingDataToSyncService;

        //private readonly IPluginManager<IBoxGeneratorServices> _paymentPluginManager;
        private readonly IShippingPluginManager _shippingPluginManager;
        private readonly IStateProvinceService _stateProvinceService;
        private readonly ICountryService _countryService;
        private readonly IProductService _productServices;

        public NetsuiteConnectorApiController(ISettingService settingService, ILogger logger, 
            IPendingDataToSyncService pendingDataToSyncService, IShippingPluginManager shippingPluginManager, ICountryService countryService, IStateProvinceService stateProvinceService,
            IProductService productServices)
        {
            _settingService = settingService;
            _logger = logger;
            _pendingDataToSyncService = pendingDataToSyncService;
            _shippingPluginManager = shippingPluginManager;
            _countryService = countryService;
            _stateProvinceService=stateProvinceService;
            _productServices = productServices;

        }
        [HttpPost("gettoken")]
        public IActionResult GetToken([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var token = "";
            try
            {
                _logger.Warning("Generate token " + model.UserId);
                token = GenerateToken(model.UserId, model.password);
            }
            catch (Exception ex)
            {
                _logger.Warning("Error  Generate token " + model.UserId, ex);
                return Ok(new { Message = "No found" });
            }


            return Ok(new { Token = token });
        }

        // POST: api/customerapi/update
        [Authorize]
        [HttpPost("updateCustomer")]
        public async Task<ActionResult> updateCustomer([FromBody] CustomerUpdateModel model)
        {
            try
            {
                if (!ModelState.IsValid || !model.updated)
                {
                    return BadRequest();
                }
                var customerIds = model.id.Split(',');

                foreach (var customerId in customerIds)
                {
                    try
                    {
                       var GetPendingDataToSync =_pendingDataToSyncService.GetPendingDataToSync(Convert.ToInt32(customerId));

                        if (GetPendingDataToSync != null)
						{
                            GetPendingDataToSync.Synchronized = false;
                            GetPendingDataToSync.UpdateDate = DateTime.Now;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(GetPendingDataToSync);
                        }
						else
						{
                            var items = new PendingDataToSync();
                            items.IdItem = Convert.ToInt32(customerId);
                            items.Synchronized = false;
                            items.Type = (int)ImporterIdentifierType.CustomerImporter; ;// 2;
                            items.UpdateDate = DateTime.Now;
                            _pendingDataToSyncService.InsertPendingDataToSync(items);
                        }
                       

                    }
                    catch (Exception ex)
                    {
                        _logger = EngineContext.Current.Resolve<ILogger>();

                        // Log any exceptions that occur during the process
                        _logger.Warning("Add Customer list to PendingDataToSync: " + customerId, ex);
                    }

					
				}


                return StatusCode(200);

            }
            catch (Exception ex)
            {
                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Warning("Error update Customer " + model.id, ex);

                return BadRequest();
            }
        }

        // POST: api/customerapi/update
        [Authorize]
        [HttpPost("updateProduct")]
        public async Task<ActionResult> updateProduct([FromBody] CustomerUpdateModel model)
        {
            try
            {
                if (!ModelState.IsValid || !model.updated)
                {
                    return BadRequest();
                }
                var customerIds = model.id.Split(',');

                foreach (var customerId in customerIds)
                {
                    try
                    {
                        var GetPendingDataToSync = _pendingDataToSyncService.GetPendingDataToSync(Convert.ToInt32(customerId));

                        if (GetPendingDataToSync != null)
                        {
                            GetPendingDataToSync.Synchronized = false;
                            GetPendingDataToSync.UpdateDate = DateTime.Now;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(GetPendingDataToSync);
                        }
                        else
                        {
                            var items = new PendingDataToSync();
                            items.IdItem = Convert.ToInt32(customerId);
                            items.Synchronized = false;
                            items.Type = (int)ImporterIdentifierType.ProductImporter; ;// 1;
                            items.UpdateDate = DateTime.Now;
                            _pendingDataToSyncService.InsertPendingDataToSync(items);
                        }


                    }
                    catch (Exception ex)
                    {
                        _logger = EngineContext.Current.Resolve<ILogger>();

                        // Log any exceptions that occur during the process
                        _logger.Warning("Add product list to PendingDataToSync: " + customerId, ex);
                    }

                }


                return StatusCode(200);

            }
            catch (Exception ex)
            {
                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Warning("Error update product " + model.id, ex);

                return BadRequest();
            }
        }


        // POST: api/customerapi/update
        [Authorize]
        [HttpPost("updateOrder")]
        public async Task<ActionResult> updateOrder([FromBody] OrderUpdateModel model)
        {
            try
            {
                if (!ModelState.IsValid || !model.updated)
                {
                    return BadRequest();
                }
                var customerIds = model.id.Split(',');

                foreach (var customerId in customerIds)
                {
                    try
                    {
                        var GetPendingDataToSync = _pendingDataToSyncService.GetPendingDataToSync(Convert.ToInt32(customerId));

                        if (GetPendingDataToSync != null)
                        {
                            GetPendingDataToSync.Synchronized = false;
                            GetPendingDataToSync.UpdateDate = DateTime.Now;
                            GetPendingDataToSync.ShippingStatus = model.ShippingStatus;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(GetPendingDataToSync);
                        }
                        else
                        {
                            var items = new PendingDataToSync();
                            items.IdItem = Convert.ToInt32(customerId);
                            items.ShippingStatus = model.ShippingStatus;
                            items.Synchronized = false;
                            items.Type = (int)ImporterIdentifierType.OrderImporter; //3;
                            items.UpdateDate = DateTime.Now;
                            _pendingDataToSyncService.InsertPendingDataToSync(items);
                        }


                    }
                    catch (Exception ex)
                    {
                        _logger = EngineContext.Current.Resolve<ILogger>();

                        // Log any exceptions that occur during the process
                        _logger.Warning("Add Order to PendingDataToSync: " + customerId, ex);
                    }

                }


                return StatusCode(200);

            }
            catch (Exception ex)
            {
                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Warning("Error update updateOrder " + model.id, ex);

                return BadRequest();
            }
        }

        // POST: api/customerapi/UpdateInvoice
        [Authorize]
        [HttpPost("updateInvoice")]
        public async Task<ActionResult> UpdateInvoice([FromBody] CustomerModel model)
        {
            try
            {
                if (!ModelState.IsValid || !model.Updated)
                {
                    return BadRequest();
                }
                var customerIds = model.CustomerId.Split(',');

                foreach (var customerId in customerIds)
                {
                    try
                    {
                        var GetPendingDataToSync = _pendingDataToSyncService.GetPendingInvocesDataToSync(Convert.ToInt32(customerId), ImporterIdentifierType.InvoicesSync);

                        if (GetPendingDataToSync != null)
                        {
                            GetPendingDataToSync.Synchronized = false;
                            GetPendingDataToSync.UpdateDate = DateTime.Now;
                            _pendingDataToSyncService.UpdatetPendingDataToSync(GetPendingDataToSync);
                        }
                        else
                        {
                            var items = new PendingDataToSync();
                            items.IdItem = Convert.ToInt32(customerId);
                            items.Synchronized = false;
                            items.Type = (int)ImporterIdentifierType.InvoicesSync;//8
                            items.UpdateDate = DateTime.Now;
                            _pendingDataToSyncService.InsertPendingDataToSync(items);
                        }


                    }
                    catch (Exception ex)
                    {
                        _logger = EngineContext.Current.Resolve<ILogger>();

                        // Log any exceptions that occur during the process
                        _logger.Warning("Add Invoice list to PendingDataToSync: " + customerId, ex);
                    }

                }


                return StatusCode(200);

            }
            catch (Exception ex)
            {
                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Warning("Error update Invoice " + model. CustomerId, ex);

                return BadRequest();
            }
        }


        [Authorize]
        [HttpPost("GetUpsRates")]
        public async Task<ActionResult> GetUpsRates([FromBody] BoxModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest();
                }


                var shippingRateComputationMethods = _shippingPluginManager
                .LoadActivePlugins(new Core.Domain.Customers.Customer(), 1, "");

                GetShippingOptionResponse GetShippingOptionRequestValue = new GetShippingOptionResponse();
                decimal rateValue = decimal.Zero;

                foreach (var srcm in shippingRateComputationMethods)
                {
                    if (srcm.PluginDescriptor.SystemName == "Shipping.UPS")
                    {

                        var GetShippingOptionRequest = new GetShippingOptionRequest();
                      
                        var ShippingMethod = model.ShippingMethod;

                        var StateFrom = _stateProvinceService.GetStateProvinceByAbbreviation(model.StateProvinceFrom);
                        var StateTo = _stateProvinceService.GetStateProvinceByAbbreviation(model.StateProvinceTo);
                        var countryCodeFrom =  _countryService.GetAllCountries().FirstOrDefault();


                        GetShippingOptionRequest.AddressFrom = model.AddressFrom;
                        GetShippingOptionRequest.CityFrom = model.CityFrom;
                        GetShippingOptionRequest.CountryFrom = countryCodeFrom;
                        GetShippingOptionRequest.StateProvinceFrom = StateFrom;
                        GetShippingOptionRequest.ZipPostalCodeFrom = model.ZipPostalCodeFrom;

                        var ShippingAddress = new Address();
                        ShippingAddress.Address1 = model.AddressTo;
                        ShippingAddress.Country = countryCodeFrom;
                        ShippingAddress.City = model.CityTo;
                        ShippingAddress.StateProvince = StateTo;
                        ShippingAddress.ZipPostalCode = model.ZipPostalCodeTo;
                        GetShippingOptionRequest.ShippingAddress = ShippingAddress;

                        // items 
                        GetShippingOptionRequest.Items = new List<PackageItem>();

						foreach (var item in model.Items)
						{
                            var ShoppingCartItem = new Core.Domain.Orders.ShoppingCartItem();
                            ShoppingCartItem.Product = _productServices.GetProductsByNetsuiteItem(item.Id);
                            ShoppingCartItem.Quantity = item.Quantity;
                            ShoppingCartItem.ProductId = ShoppingCartItem.Product.Id;
                           var PackageItem = new PackageItem(ShoppingCartItem, item.Quantity);

                            GetShippingOptionRequest.Items.Add(PackageItem);
                        }
                        


                        var pack = srcm.GetBoxesPackingData(GetShippingOptionRequest);

                        GetShippingOptionRequestValue = srcm.GetShippingOptions(GetShippingOptionRequest);
                        var NameShippingMethod = "";
                        //if (model.ShippingMethod == _settingService.GetSetting("ShippingUpsGroup.Name").Value)// "UPS Ground")
                        //    NameShippingMethod = _settingService.GetSetting("ShippingUpsGroup.id").Value; // "1611";
                        //if (model.ShippingMethod == _settingService.GetSetting("ShippingNextDayAir.Name").Value)  //"UPS Next Day Air")
                        //    NameShippingMethod = _settingService.GetSetting("ShippingNextDayAir.id").Value;// "2521";
                        //if (model.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirEarlyAm.Name").Value)//"UPS Next Day Air Early AM")
                        //    NameShippingMethod = _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value;//"2522";
                        //if (model.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirSaver.Name").Value)// "UPS Next Day Air Saver")
                        //    NameShippingMethod = _settingService.GetSetting("ShippingNextDayAirSaver.id").Value;//"2515";
                        //if (model.ShippingMethod == _settingService.GetSetting("ShippingSecondDay.Name").Value) //"UPS Second Day Air")
                        //    NameShippingMethod = _settingService.GetSetting("ShippingSecondDay.id").Value;// "2520";
                        //if (model.ShippingMethod == _settingService.GetSetting("ShippingSecondDayAirAm.Name").Value) //"UPS Second Day Air AM")
                        //    NameShippingMethod = _settingService.GetSetting("ShippingSecondDayAirAm.id").Value;//"2513";
                        //if (model.ShippingMethod == _settingService.GetSetting("Shipping3DaySelect.Name").Value) //"UPS 3 Day Select")
                        //    NameShippingMethod = _settingService.GetSetting("Shipping3DaySelect.id").Value;//"2514";


                        if (model.ShippingMethod == _settingService.GetSetting("ShippingUpsGroup.id").Value)// "UPS Ground")
                            NameShippingMethod = _settingService.GetSetting("ShippingUpsGroup.Name").Value; // "1611";
                        if (model.ShippingMethod == _settingService.GetSetting("ShippingNextDayAir.id").Value)  //"UPS Next Day Air")
                            NameShippingMethod = _settingService.GetSetting("ShippingNextDayAir.Name").Value;// "2521";
                        if (model.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirEarlyAm.id").Value)//"UPS Next Day Air Early AM")
                            NameShippingMethod = _settingService.GetSetting("ShippingNextDayAirEarlyAm.Name").Value;//"2522";
                        if (model.ShippingMethod == _settingService.GetSetting("ShippingNextDayAirSaver.id").Value)// "UPS Next Day Air Saver")
                            NameShippingMethod = _settingService.GetSetting("ShippingNextDayAirSaver.Name").Value;//"2515";
                        if (model.ShippingMethod == _settingService.GetSetting("ShippingSecondDay.id").Value) //"UPS Second Day Air")
                            NameShippingMethod = _settingService.GetSetting("ShippingSecondDay.Nam").Value;// "2520";
                        if (model.ShippingMethod == _settingService.GetSetting("ShippingSecondDayAirAm.id").Value) //"UPS Second Day Air AM")
                            NameShippingMethod = _settingService.GetSetting("ShippingSecondDayAirAm.id").Value;//"2513";
                        if (model.ShippingMethod == _settingService.GetSetting("Shipping3DaySelect.Name").Value) //"UPS 3 Day Select")
                            NameShippingMethod = _settingService.GetSetting("Shipping3DaySelect.id").Value;//"2514";

                        var Json=  JsonConvert.SerializeObject(GetShippingOptionRequestValue.ShippingOptions);

                        rateValue = GetShippingOptionRequestValue.ShippingOptions.Where(r=>r.Name== NameShippingMethod).FirstOrDefault().Rate;
                    }
                }

                return Ok(new { rate = rateValue });

            }
            catch (Exception ex)
            {
                _logger = EngineContext.Current.Resolve<ILogger>();

                _logger.Warning("Error update NN box generator ", ex);

                return BadRequest();
            }
        }

        #region Private methods

        private string GenerateToken(string userId, string password)
        {
            var userIdSetting = _settingService.GetSetting("netsuiteconnectorsettings.api.userId").Value;
            var passwordSetting = _settingService.GetSetting("netsuiteconnectorsettings.api.password").Value;
            var mySecretID = _settingService.GetSetting("netsuiteconnectorsettings.api.secretid").Value;

            if (userId == userIdSetting && password == passwordSetting)
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(mySecretID);

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(new Claim[]
                    {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim(ClaimTypes.Name, password)
                    }),
                    Expires = DateTime.UtcNow.AddHours(1), // Token expiration time
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
                };

                var token = tokenHandler.CreateToken(tokenDescriptor);
                return tokenHandler.WriteToken(token);
            }
            else
            {
                return "Invalid data";

            }

        }

        //private void GetBoxesPacking(GetShippingOptionRequest shippingOptionRequest)
        //{
        //    var BoxGenerator = _paymentPluginManager.LoadPluginBySystemName("Misc.NNBoxGenerator", shippingOptionRequest.Customer, shippingOptionRequest.StoreId);

        //    GetInsuranceSurcharge();

        //    GetShippingOptionBoxGenerator = BoxGenerator.GetBoxesPacking(shippingOptionRequest.Items.ToList(), PackingTypeNNBoxGenerator, MarginError);
        //}

        #endregion
    }


    public class TokenRequestModel
    {
        public string UserId { get; set; }
        public string password { get; set; }
    }
    public class CustomerUpdateModel
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("updated")]
        public bool updated { get; set; }
    }

    public class CustomerModel
    {
        [JsonProperty("CustomerId")]
        public string CustomerId { get; set; }

        [JsonProperty("updated")]
        public bool Updated { get; set; }
    }

    public class OrderUpdateModel
    {
        [JsonProperty("id")]
        public string id { get; set; }

        [JsonProperty("ShippingStatus")]
        public string ShippingStatus { get; set; }

        [JsonProperty("updated")]
        public bool updated { get; set; }
    }

    public class BoxModel
    {
        public BoxModel()
        {
            Items = new List<ItemModel>();
        }


        [JsonProperty("items")]
        public List<ItemModel> Items { get; set; }


        [JsonProperty("FromState")]
        public string StateProvinceFrom { get; set; }

        [JsonProperty("FromZip")]
        public string ZipPostalCodeFrom { get; set; }

        [JsonProperty("FromCity")]
        public string CityFrom { get; set; }

        [JsonProperty("FromStreet")]
        public string AddressFrom { get; set; }



        [JsonProperty("ToState")]
        public string StateProvinceTo { get; set; }

        [JsonProperty("ToZip")]
        public string ZipPostalCodeTo { get; set; }

        [JsonProperty("ToCity")]
        public string CityTo { get; set; }

        [JsonProperty("ToStreet")]
        public string AddressTo { get; set; }



        [JsonProperty("ShippingMethod")]
        public string ShippingMethod { get; set; }
    }

    public class ItemModel
    {
        [JsonProperty("Id")]
        public string Id { get; set; }

        [JsonProperty("Width")]
        public float Width { get; set; }

        [JsonProperty("Length")]
        public float Length { get; set; }

        [JsonProperty("Weight")]
        public float Weight { get; set; }

        [JsonProperty("Height")]
        public float Height { get; set; }

        [JsonProperty("quantity")]
        public int Quantity { get; set; }

        [JsonProperty("quantityincrement")]
        public float QuantityIncrement { get; set; }
    }
}

