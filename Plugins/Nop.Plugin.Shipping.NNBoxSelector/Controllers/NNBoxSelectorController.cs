using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.NNBoxSelector.Models;
using Nop.Plugin.Shipping.NNBoxSelector.Services;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using Nop.Core.Domain.Common;
using Nop.Web.Framework.Models.Extensions;
using System.Linq;
using System;
using System.Collections.Generic;
using Nop.Plugin.Shipping.NNBoxSelector.Models.AlgorithmBase;
using Nop.Services.Shipping;

namespace Nop.Plugin.Shipping.NNBoxSelector.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class NNBoxSelectorController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly NNBoxselectorSettings _nnDeliverySettings;
        private readonly ISettingService _settingService;
        private readonly IBoxPackingService _boxPackingService;


        #endregion

        #region Ctor

        public NNBoxSelectorController(IPermissionService permissionService, 
            NNBoxselectorSettings nnDeliverySettings,
            ISettingService settingService,
            IBoxPackingService boxPackingService)
        {
            _permissionService = permissionService;
            _nnDeliverySettings = nnDeliverySettings;
            _settingService = settingService;
            _boxPackingService = boxPackingService;
        }

        #endregion

        #region Methods
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var nNBoxSettings = new NNBoxselectorSettings
            {
                InsuranceSurcharge = _nnDeliverySettings.InsuranceSurcharge
            };
            var model = new ConfigurationModel
            {
                NNBoxSettings = nNBoxSettings
            };            

            return View("~/Plugins/Shipping.NNBoxSelector/Views/Configure.cshtml", model);
        }

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        [HttpPost, ActionName("Configure")]
        [FormValueRequired("save")]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            //save settings
            //_nnDeliverySettings.FixedRate = model.FixedRate;
            _settingService.SaveSetting(_nnDeliverySettings);

            return View("~/Plugins/Shipping.NNBoxSelector/Views/Configure.cshtml", model);
        }

        #endregion

        #region NN Box Selector

        [HttpPost]
        public virtual IActionResult Add([Validate] BSBoxModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
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
                WeigthAvailable  = model.Weigth,
                Width = model.Width,
                Height = model.Height,
                VolumenBox = model.VolumenBox,
                Active = model.Active
            };

            _boxPackingService.InsertBox(BSBox);

            return Json(new { Result = true });
        }


        [HttpPost]
        public virtual IActionResult SaveInsuranceSurcharge(NNBoxselectorSettings model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            //if (model.InsuranceSurcharge != null)
            //    model.InsuranceSurcharge = 0;
            //if (model.Location != null)
            //    model.Location = model.Location.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }
            //settings
            var settings = new NNBoxselectorSettings
            {
                InsuranceSurcharge = model.InsuranceSurcharge, 
                
            };
            _settingService.SaveSetting(settings);

           // var GetShippingOptions = NNBoxSelectorComputationMethod.Pack(new List<Container>(), new GetShippingOptionRequest());


            return Json(new { Result = true });
        }


        [HttpPost]
        public IActionResult List(BoxesSelectorSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
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
                        Length= dr.Length,
                        Name=dr.Name, 
                        Weigth= dr.WeigthAvailable,
                        Width= dr.Width, 
                        VolumenBox=dr.VolumenBox
                        
                    };

                    return deliveryRoutesModel;
                });
            });

            return Json(model);
        }

        [HttpPost]
        public IActionResult Edit(BSBoxModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var deliveryRoute = _boxPackingService.GetById(model.Id) ??
                throw new ArgumentException("No delivery route found with specified Id");

            deliveryRoute.Name = model.Name;
            deliveryRoute.Length = model.Length;
            deliveryRoute.WeigthAvailable = model.Weigth;
            deliveryRoute.Width = model.Width;
            deliveryRoute.Height = model.Height;
            deliveryRoute.VolumenBox = model.Length * model.Width * model.Height;
            deliveryRoute.Active = model.Active;

            _boxPackingService.UpdateDeliveryRoute(deliveryRoute);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var deliveryRoute = _boxPackingService.GetById(id) ??
                throw new ArgumentException("No Boxes found with specified Id");

            _boxPackingService.DeleteBoxes(deliveryRoute);

            return new NullJsonResult();
        }

        #endregion
    }
}
