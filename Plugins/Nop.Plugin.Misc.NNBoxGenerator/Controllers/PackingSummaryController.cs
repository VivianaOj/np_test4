using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.NNBoxGenerator.Models;
using Nop.Plugin.Misc.NNBoxGenerator.Services;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Security;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Framework;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Nop.Plugin.Misc.NNBoxGenerator.Controllers
{
    public class PackingSummaryController : BaseAdminController
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

        public PackingSummaryController(IPermissionService permissionService,
            NNBoxGeneratorSettings nnDeliverySettings,
            ISettingService settingService,
            IBoxPackingService boxPackingService, IProductService productServices,
            ICompanyService companyServices)
        {
            _permissionService = permissionService;
            _nnDeliverySettings = nnDeliverySettings;
            _settingService = settingService;
            _boxPackingService = boxPackingService;
            _productServices = productServices;
            _companyServices = companyServices;
        }

        #endregion


        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult PackingSummary()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var BoxGenerator = _boxPackingService.GetOrderSummaryList().OrderByDescending(r => r.DateUpdated); ;
            var packing = new PackingSummaryModel();

            foreach (var dr in BoxGenerator)
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
                var email = "Guest";
                if (!string.IsNullOrEmpty(dr.Customer.Email))
                    email = dr.Customer.Email;

                var DetailBox = new BSPackedOrderModel
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
                    CustomerEmail = email,
                    IsCommercial = dr.IsCommercial,
                    Name = dr.Customer.BillingAddress.FirstName + " " + dr.Customer.BillingAddress.LastName

                };

                packing.BoxGeneratorList.Add(DetailBox);
            }

            return View("~/Plugins/Misc.NNBoxGenerator/Views/PackingSummary.cshtml", packing);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("List")]
        public IActionResult List(PackingSummarySearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var BoxGenerator = _boxPackingService.SearchPacking(searchModel.CustomerName, searchModel.OrderId, searchModel.Page - 1, searchModel.PageSize);

            var packingList = BoxGenerator.OrderByDescending(r=>r.DateUpdated);


            var model = new PackingSummaryListModel().PrepareToGrid(searchModel, BoxGenerator, () =>
            {
                return packingList.Select(dr =>
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

                        //var company = new Company();

                        //if (dr.Order != null)
                        //{
                        //    if(dr.Order.CompanyId != null)
                        //    {
                        //        int idNetsuite = Convert.ToInt32(dr.Order.CompanyId);
                        //        company = _companyServices.GetCompanyById(idNetsuite);
                        //    }
                        //}
                        var email = "Guest";
                        if (!string.IsNullOrEmpty(dr.Customer.Email))
                            email = dr.Customer.Email;

                        DetailBox = new BSPackedOrderModel
                        {
                            Active = dr.Active,
                            CustomerId = dr.CustomerId,
                            Id = dr.Id,
                            OrderId = dr.OrderId,
                            DateCreated = dr.DateCreated.ToShortDateString(),
                            DateUpdated = dr.DateUpdated.ToShortDateString(),
                            CompanyName = companyName,
                            CustomerEmail = email,
                            IsCommercial = dr.IsCommercial,
                            Name = dr.Customer.BillingAddress.FirstName + " " + dr.Customer.BillingAddress.LastName

                        };

                    }
                    
                    return DetailBox;
                });
            });

            return Json(model);
        }


        //[AuthorizeAdmin]
        //[Area(AreaNames.Admin)]
        //[HttpPost, ActionName("List")]
        //public IActionResult List(PackingSummarySearchModel searchModel)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedDataTablesJson();

        //    var BoxGenerator = _boxPackingService.SearchPacking(searchModel.CustomerName, searchModel.OrderId, searchModel.Page - 1, searchModel.PageSize);

        //    var packingList = BoxGenerator;

        //    var model = new PackingSummaryListModel().PrepareToGrid(searchModel, BoxGenerator, () =>
        //    {
        //        return packingList.Select(dr =>
        //        {
        //            var BoxInfo = new BSBox();
        //            BSItemPackedBox Packing = _boxPackingService.GetItemPackedBoxId(dr.Id);

        //            //var GroupPacking = Packing.grou

        //            if (dr.Container != null)
        //                BoxInfo = _boxPackingService.GetById(dr.Container.ID);

        //            var BoxContentWeight = dr.PercentItemWeightPacked + BoxInfo.WeigthBox;

        //            var DimensionHtml = PrepareModelBSBoxHtml(BoxInfo);


        //            var OrderInformation = Packing;

        //            var DetailBox = new BoxGenerator
        //            {
        //                BoxName = BoxInfo?.Name,
        //                BoxSize = BoxInfo?.Height + " (Height) x " + BoxInfo?.Width + " (Width) x " + BoxInfo?.Length + " (Length) ",
        //                BoxTotalWeight = dr.PercentItemWeightPacked.ToString(),
        //                BoxContentWeight = BoxContentWeight.ToString(),
        //                Id = dr.Id,
        //                Active = dr.Active,
        //                VolumenBox = dr.TotalVolumePacked,
        //                Height = Packing.FinalHeight,
        //                Width = Packing.FinalWidth,
        //                Length = Packing.FinalLength,
        //                DimensionHtml = DimensionHtml
        //                //PackedItems = Products
        //            };
        //            return DetailBox;
        //        });
        //    });

        //    return Json(model);
        //}

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Edit")]
        public IActionResult Edit(BoxGenerator model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var Packing = _boxPackingService.GetBSPackedOrderById(model.Id) ??
                throw new ArgumentException("No delivery route found with specified Id");

            Packing.Active = model.Active;
            

            _boxPackingService.UpdateBSPackedOrder(Packing);

            return new NullJsonResult();
        }

        private  string PrepareModelBSBoxHtml(BSBox model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var addressHtmlSb = new StringBuilder("<div>");


            if (model.Height != 0)
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.Height.ToString()));

            if (model.Width != 0)
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.Width.ToString()));

            if (model.Length != 0)
                addressHtmlSb.AppendFormat("{0}", WebUtility.HtmlEncode(model.Length.ToString()));

            addressHtmlSb.Append("</div>");

            return addressHtmlSb.ToString();
        }

        //[AuthorizeAdmin]
        //[Area(AreaNames.Admin)]
        //[HttpPost, ActionName("List")]
        //public IActionResult List(PackingSummarySearchModel searchModel)
        //{
        //    if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
        //        return AccessDeniedDataTablesJson();

        //    var BoxGenerator = _boxPackingService.ListPacking(searchModel.CustomerName, searchModel.OrderId, searchModel.Page - 1, searchModel.PageSize);
        //    var packingList = BoxGenerator;
        //    var GroupPacking = BoxGenerator.GroupBy(r => r.OrderId);

        //    List<PackingSummaryListModel> boxGenerators = new List<PackingSummaryListModel>();

        //    foreach (var item in GroupPacking)
        //    {
        //        var groupByOrder = item.GroupBy(r => r.CustomerId).ToList();
        //        var BoxName = "";
        //        var BoxSize = "";
        //        var BoxTotalWeight = "";
        //        var BoxContentWeight = "";
        //        var Active = "";
        //        var VolumenBox = "";
        //        var Height = "";
        //        var Width = "";
        //        var Length = "";

        //        foreach (var x in groupByOrder)
        //        {
        //            var BoxInfo = new BSBox();

        //            foreach (var y in x)
        //            {
        //                var dr = JsonConvert.DeserializeObject<AlgorithmPackingResult>(y.ContainerPackingResult);

        //                if (y.ContainerId != null)
        //                    BoxInfo = _boxPackingService.GetById(y.ContainerId);

        //                // BoxContentWeight = dr.PercentItemWeightPacked + BoxInfo.WeigthBox;
        //                var DimensionHtml = PrepareModelBSBoxHtml(BoxInfo);





        //            }


        //        }

        //        //var DetailBox = new BoxGenerator
        //        //{
        //        //    BoxName = BoxInfo?.Name,
        //        //    BoxSize = BoxInfo?.Height + " (Height) x " + BoxInfo?.Width + " (Width) x " + BoxInfo?.Length + " (Length) ",
        //        //    BoxTotalWeight = dr.PercentItemWeightPacked.ToString(),
        //        //    BoxContentWeight = BoxContentWeight.ToString(),
        //        //    Id = dr.Id,
        //        //    Active = dr.Active,
        //        //    VolumenBox = dr.TotalVolumePacked,
        //        //    Height = y.FinalHeight,
        //        //    Width = y.FinalWidth,
        //        //    Length = y.FinalLength,
        //        //    DimensionHtml = DimensionHtml
        //        //    //PackedItems = Products
        //        //};
        //        //boxGenerators.Add(DetailBox);

        //    }

        //    var model = new PackingSummaryListModel().PrepareToGrid(searchModel, BoxGenerator, () => {

        //        return GroupPacking.Select(dr =>
        //        {
        //            var groupByOrder = dr.GroupBy(r => r.CustomerId).ToList();
        //            var BoxName = "";
        //            var BoxSize = "";
        //            var BoxTotalWeight = "";
        //            var BoxContentWeight = "";
        //            var Active = "";
        //            var VolumenBox = "";
        //            var Height = "";
        //            var Width = "";
        //            var Length = "";
        //            var DimensionHtml = "";
        //            Order order = new Order();
        //            BSBox BoxInfo = new BSBox();

        //            foreach (var x in groupByOrder)
        //            {
        //                foreach (var y in x)
        //                {
        //                    if (y.Order != null)
        //                        order = y.Order;

        //                    var containerInfo = JsonConvert.DeserializeObject<AlgorithmPackingResult>(y.ContainerPackingResult);

        //                    if (y.ContainerId != null)
        //                        BoxInfo = _boxPackingService.GetById(y.ContainerId);

        //                    // BoxContentWeight = dr.PercentItemWeightPacked + BoxInfo.WeigthBox;
        //                    DimensionHtml = DimensionHtml + PrepareModelBSBoxHtml(BoxInfo);

        //                }


        //            }
        //            var DetailBox = new BoxGenerator
        //            {
        //                DimensionHtml = DimensionHtml,
        //                OrderId = order.Id

        //            };
        //            return DetailBox;
        //        });
        //    });


        //    return Json(model);
        //}
    }
}
