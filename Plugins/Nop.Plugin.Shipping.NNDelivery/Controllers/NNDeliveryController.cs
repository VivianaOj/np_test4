using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Shipping.NNDelivery.Domain;
using Nop.Plugin.Shipping.NNDelivery.Models;
using Nop.Plugin.Shipping.NNDelivery.Services;
using Nop.Services.Configuration;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Models.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using System;
using System.Linq;

namespace Nop.Plugin.Shipping.NNDelivery.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.Admin)]
    public class NNDeliveryController : BasePluginController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly NNDeliverySettings _nnDeliverySettings;
        private readonly ISettingService _settingService;
        private readonly IDeliveryRoutesService _deliveryRoutesService;

        #endregion

        #region Ctor

        public NNDeliveryController(IPermissionService permissionService, 
            NNDeliverySettings nnDeliverySettings,
            ISettingService settingService,
            IDeliveryRoutesService deliveryRoutesService)
        {
            _permissionService = permissionService;
            _nnDeliverySettings = nnDeliverySettings;
            _settingService = settingService;
            _deliveryRoutesService = deliveryRoutesService;
        }

        #endregion

        #region Methods

        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                FixedRate = _nnDeliverySettings.FixedRate
            };            

            return View("~/Plugins/Shipping.NNDelivery/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return Content("Access denied");

            //save settings
            _nnDeliverySettings.FixedRate = model.FixedRate;
            _settingService.SaveSetting(_nnDeliverySettings);

            return View("~/Plugins/Shipping.NNDelivery/Views/Configure.cshtml", model);
        }

        #endregion

        #region Delivery Routes

        [HttpPost]
        public virtual IActionResult Add([Validate] DeliveryRoutesModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings))
                return AccessDeniedView();

            if (model.Name != null)
                model.Name = model.Name.Trim();
            if (model.Location != null)
                model.Location = model.Location.Trim();

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            var deliveryRoutes = new DeliveryRoutes {
                Name = model.Name,
                Location = model.Location,
                Available = model.Available,
                Minimum = model.Minimum,
                ValueToSend = model.ValueToSend,
                IdValueToSend = model.IdValueToSend
            };

            _deliveryRoutesService.InsertDeliveryRoute(deliveryRoutes);

            return Json(new { Result = true });
        }

        [HttpPost]
        public IActionResult List(DeliveryRoutesSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedDataTablesJson();

            var deliveryRoutes = _deliveryRoutesService.SearchDeliveryRoutes(searchModel.Name, searchModel.Location, searchModel.Page - 1, searchModel.PageSize);

            var model = new DeliveryRoutesListModel().PrepareToGrid(searchModel, deliveryRoutes, () =>
            {
                return deliveryRoutes.Select(dr =>
                {
                    var deliveryRoutesModel = new DeliveryRoutesModel
                    {
                        Id = dr.Id,
                        Name = dr.Name,
                        Location = dr.Location,
                        Available = dr.Available,
                        Minimum = dr.Minimum,
                        ValueToSend= dr.ValueToSend,
                        IdValueToSend=dr.IdValueToSend
                    };

                    return deliveryRoutesModel;
                });
            });

            return Json(model);
        }

        [HttpPost]
        public IActionResult Edit(DeliveryRoutesModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var deliveryRoute = _deliveryRoutesService.GetById(model.Id) ??
                throw new ArgumentException("No delivery route found with specified Id");

            deliveryRoute.Name = model.Name;
            deliveryRoute.Location = model.Location;
            deliveryRoute.Available = model.Available;
            deliveryRoute.Minimum = model.Minimum;
            deliveryRoute.ValueToSend = model.ValueToSend;
            deliveryRoute.IdValueToSend = model.IdValueToSend;

            _deliveryRoutesService.UpdateDeliveryRoute(deliveryRoute);

            return new NullJsonResult();
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageShippingSettings) && !_permissionService.Authorize(StandardPermissionProvider.ManageProducts))
                return AccessDeniedView();

            var deliveryRoute = _deliveryRoutesService.GetById(id) ??
                throw new ArgumentException("No delivery route found with specified Id");

            _deliveryRoutesService.DeleteDeliveryRoute(deliveryRoute);

            return new NullJsonResult();
        }

        #endregion
    }
}
