using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.NNBoxGenerator.Models;
using Nop.Plugin.Misc.NNBoxGenerator.Services;
using Nop.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Media;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using static Nop.Web.Areas.Admin.Models.Orders.OrderModel;
using Nop.Core.Domain.Orders;

namespace Nop.Plugin.Misc.NNBoxGenerator.Controllers
{
    public class NNBoxGeneratorController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly NNBoxGeneratorSettings _nnDeliverySettings;
        private readonly ISettingService _settingService;
        private readonly IBoxPackingService _boxPackingService;
        private readonly IProductService _productServices;
        private readonly ICompanyService _companyServices;

        #endregion

        #region Ctor

        public NNBoxGeneratorController(IPermissionService permissionService,
            NNBoxGeneratorSettings nnDeliverySettings,
            ISettingService settingService,
            IBoxPackingService boxPackingService, IProductService productServices, ICompanyService companyServices)
        {
            _permissionService = permissionService;
            _nnDeliverySettings = nnDeliverySettings;
            _settingService = settingService;
            _boxPackingService = boxPackingService;
            _productServices = productServices;
            _companyServices = companyServices;
        }

        #endregion

        #region Utilities

        private void PrepareModel(ConfigurationModel model)
        {
            var NNBoxGeneratorSettings = _settingService.LoadSetting<NNBoxGeneratorSettings>();

            model.NNBoxSettings.InsuranceSurcharge = NNBoxGeneratorSettings.InsuranceSurcharge;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator))
                return AccessDeniedView();

            var AvaliablePackingTypes = PackingType.PackByDimensions.ToSelectList(false)
                .Select(item => new SelectListItem(item.Text, item.Value)).ToList();

            var nNBoxSettings = new NNBoxGeneratorSettings
            {
                InsuranceSurcharge = _nnDeliverySettings.InsuranceSurcharge,
                MarginError = _nnDeliverySettings.MarginError, 
            };
            var model = new ConfigurationModel
            {
                NNBoxSettings = nNBoxSettings,
                AvaliablePackingTypes = AvaliablePackingTypes
            };

            PrepareModel(model);

            return View("~/Plugins/Misc.NNBoxGenerator/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator))
                return Content("Access denied");

            
            _settingService.SaveSetting(_nnDeliverySettings);

            return View("~/Plugins/Misc.NNBoxGenerator/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult PackingSummary()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator))
                return AccessDeniedView();

            var BoxGenerator = _boxPackingService.GetOrderSummaryList().OrderByDescending(r=>r.DateUpdated);
            var packing = new PackingSummaryModel();

            foreach (var dr in BoxGenerator)
            {
                var DetailBox = new BSPackedOrderModel();

                if (dr.Customer != null)
                {
                    var companyName = "";
                    if (dr.Customer.Companies.Any())
                    {
                        var company = dr.Customer.CompanyCustomerMappings.Where(r => r.DefaultCompany).FirstOrDefault();
                        if (company != null)
                            companyName = company.Company.CompanyName;
                    }
                    else
                    {
                        companyName = dr.Customer.BillingAddress?.Company;
                    }

                    DetailBox = new BSPackedOrderModel
                    {
                        Active = dr.Active,
                        Customer = dr.Customer,
                        CustomerId = dr.CustomerId,
                        Id = dr.Id,
                        Order = dr.Order,
                        OrderId = dr.OrderId,
                        DateCreated = dr.DateCreated.ToShortDateString(),
                        DateUpdated = dr.DateUpdated.ToShortDateString(),
                        CompanyName = companyName,
                        IsCommercial = dr.IsCommercial,
                        Name = dr.Customer.BillingAddress.FirstName + " " + dr.Customer.BillingAddress.LastName
                    };

                    packing.BoxGeneratorList.Add(DetailBox);
                }
            }

            return View("~/Plugins/Misc.NNBoxGenerator/Views/PackingSummary.cshtml", packing);
        }
        #endregion

        #region NN Box Selector

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Add")]
        public virtual IActionResult Add([Validate] BSBoxModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            //if (model.Location != null)
            //    model.Location = model.Location.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var BSBox = new BSBox
            {
                Name = model.Name,
                Length = model.Length,
                WeigthAvailable = model.WeigthAvailable,
                WeigthBox = model.WeigthBox,
                Width = model.Width,
                Height = model.Height,
                VolumenBox = model.VolumenBox,
                Active = model.Active
            };

            _boxPackingService.InsertBox(BSBox);

            return Json(new { Result = true });
        }


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("SaveInsuranceSurcharge")]
        public virtual IActionResult SaveInsuranceSurcharge(NNBoxGeneratorSettings model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator))
                return AccessDeniedView();


            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }
            //settings
            var settings = new NNBoxGeneratorSettings
            {
                InsuranceSurcharge = model.InsuranceSurcharge,
                MarginError = model.MarginError, 
                PackingType = model.PackingType

            };
            _settingService.SaveSetting(settings);

            return Json(new { Result = true });
        }


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("List")]
        public IActionResult List(BoxesSelectorSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator))
                return AccessDeniedDataTablesJson();

            var deliveryRoutes = _boxPackingService.SearchBoxes(searchModel.Name, searchModel.Name, searchModel.Page - 1, searchModel.PageSize);

            var model = new BoxSelectorListModel().PrepareToGrid(searchModel, deliveryRoutes, () =>
            {
                return deliveryRoutes.Select(dr =>
                {
                    var deliveryRoutesModel = new BSBoxModel
                    {
                        Id = dr.Id,
                        Active = dr.Active,
                        Height = dr.Height,
                        Length = dr.Length,
                        Name = dr.Name,
                        WeigthAvailable = dr.WeigthAvailable,
                        WeigthBox = dr.WeigthBox,
                        WeigthTotalBox = dr.WeigthBox+ dr.WeigthAvailable,
                        Width = dr.Width,
                        VolumenBox = dr.VolumenBox

                    };

                    return deliveryRoutesModel;
                });
            });

            return Json(model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Edit")]
        public IActionResult Edit(BSBoxModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator) )
                return AccessDeniedView();

            var deliveryRoute = _boxPackingService.GetById(model.Id) ??
                throw new ArgumentException("No delivery route found with specified Id");

            deliveryRoute.Name = model.Name;
            deliveryRoute.Length = model.Length;
            deliveryRoute.WeigthAvailable = model.WeigthAvailable;
            deliveryRoute.WeigthBox = model.WeigthBox;
            deliveryRoute.Width = model.Width;
            deliveryRoute.Height = model.Height;
            deliveryRoute.VolumenBox = model.Length * model.Width * model.Height;
            deliveryRoute.Active = model.Active;

            _boxPackingService.UpdateBSBox(deliveryRoute);

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Delete")]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator) )
                return AccessDeniedView();

            var deliveryRoute = _boxPackingService.GetById(id) ??
                throw new ArgumentException("No Boxes found with specified Id");

            _boxPackingService.DeleteBoxes(deliveryRoute);

            return new NullJsonResult();
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public virtual IActionResult View(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.NNBoxGenerator))
                return AccessDeniedView();

            //try to get Packing with the specified id
            var BoxGeneratorList = _boxPackingService.GetBSItemPackedBoxList(id);

            var PackingOrderById = _boxPackingService.PackingOrderById(id);
            
            if (BoxGeneratorList.Count==0)
                return RedirectToAction("List");

            var itemsBoxGenerator = new BoxGeneratorList();
            List<ShippingOption> ShippingOptionInfo = new List<ShippingOption>();

            foreach (var item in BoxGeneratorList)
            {
                //try to get Packing with the specified id
                var BoxGenerator = _boxPackingService.PackingById(item.Id);
                //var shippingOptions = Json.des

                //prepare model
                var BoxInfo = new BSBox();

                if (item.ContainerId != 0)
                    BoxInfo = _boxPackingService.GetById(item.ContainerId);

                var BoxContentWeight = BoxGenerator.PercentItemWeightPacked;

                var BoxContentWeightWithoutBox = BoxGenerator.PercentItemWeightPacked - BoxInfo.WeigthBox;

                var Products = new List<ItemProductSummary>();

                if (BoxGenerator.IsAsShip)
                {
                    var productBox = _boxPackingService.GetBSItemPackList(BoxGenerator.Id);
                    var ProductsGroup = productBox.GroupBy(r => r.ProductId);

                    foreach (var prod in ProductsGroup)
                    {
                        var product = _productServices.GetProductById(prod.Key);
                        if (product != null)
                        {
                            var ProductItem = new ItemProductSummary();
                            ProductItem.Id = product.Id;
                            ProductItem.ProductName = product.Name;
                            ProductItem.Sku = product.Sku;
                            ProductItem.Quantity = prod.Count() * product.IncrementQuantity; 

                            Products.Add(ProductItem);
                        }
                    }
                    //var product = _productServices.GetProductById(BoxGenerator.Id);

                }
                else
                {
                    var ProductsGroup = BoxGenerator.PackedItems.GroupBy(r => r.ID);

                    foreach (var r in ProductsGroup)
                    {
                        var product = _productServices.GetProductById(r.Key);
                        if (product != null)
                        {
                            var ProductItem = new ItemProductSummary();
                            ProductItem.Id = product.Id;
                            ProductItem.ProductName = product.Name;
                            ProductItem.Sku = product.Sku;
                            ProductItem.Quantity = r.Count() * product.IncrementQuantity;
                            Products.Add(ProductItem);
                        }
                    }
                }

               

                var Packing = _boxPackingService.GetItemPackedBoxId(BoxGenerator.Id);

                var company = new Company();
                decimal FinalHeight = 0;
                decimal FinalWidth = 0;
                decimal FinalLength = 0;
                int FinalWeight = 0;
                Customer Customer = new Customer();
                var Order = new Order();

                if (Packing != null)
                {
                    if (Packing.Order != null)
                    {
                        if (Packing.Order.CompanyId != null)
                        {
                            int idNetsuite = Convert.ToInt32(Packing.Order.CompanyId);
                            company = _companyServices.GetCompanyById(idNetsuite);
                        }
                    }
                    itemsBoxGenerator.Customer = Packing.Customer;
                    itemsBoxGenerator.Order = Packing.Order;
                    itemsBoxGenerator.Company = company;

                    FinalHeight = Packing.FinalHeight;
                    FinalWidth = Packing.FinalWidth;
                    FinalLength = Packing.FinalLength;
                    FinalWeight = Convert.ToInt32(Math.Ceiling(BoxContentWeight));
                     Customer = Packing.Customer;
                     Order = Packing.Order;
                }

                 
                var BoxGeneratorModel = new BoxGenerator
                {
                    BoxName = BoxInfo?.Name,
                    BoxSize = BoxInfo?.Height + " in x " + BoxInfo?.Width + " in x " + BoxInfo?.Length + " in ",
                    BoxTotalWeight = BoxGenerator.PercentItemWeightPacked.ToString(),
                    FinalBoxTotalWeight = FinalWeight,
                    BoxContentWeight = BoxContentWeightWithoutBox.ToString(),
                    Active = BoxInfo.Active,
                    VolumenBox = BoxInfo.VolumenBox,
                    Height = FinalHeight,
                    Width =FinalWidth,
                    Length = FinalLength,
                    Id = BoxGenerator.Id,
                    PackedItems = Products,
                    Customer = Customer,
                    Order = Order,
                    Company = company,
                    IsAsShip = BoxGenerator.IsAsShip, 
                    TotalVolumenBox = BoxGenerator.TotalVolumePacked,
                    PercentBoxVolumePacked = BoxGenerator.PercentContainerVolumePacked.ToString()

                };
                itemsBoxGenerator.BoxGenerator.Add(BoxGeneratorModel);

                if (BoxGenerator.IsAsShip)
                {
                    foreach (var ship in BoxGenerator.ShippingOption)
                    {
                        var updateShippingOption = ShippingOptionInfo.Where(r => r.Name == ship.Name).FirstOrDefault();

                        if (updateShippingOption != null)
                            ship.Rate = updateShippingOption.Rate + ship.Rate;
                    }
                }

               // ShippingOptionInfo = BoxGenerator.ShippingOption;
            }
            itemsBoxGenerator.ShippingOptions = PackingOrderById.ShippingOption;

            return View("~/Plugins/Misc.NNBoxGenerator/Views/View.cshtml", itemsBoxGenerator);
        }
        #endregion

        public enum PackingType
        {
            /// <summary>
            /// Pack by volume
            /// </summary>
            PackByVolume = 1,

            /// <summary>
            /// Pack by dimensions
            /// </summary>
            PackByDimensions = 2,


        }
    }
}
