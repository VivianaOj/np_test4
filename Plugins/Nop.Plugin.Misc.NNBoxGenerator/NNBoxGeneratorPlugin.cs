using System.Linq;
using Nop.Core;
using Nop.Core.Domain.Tasks;
using Nop.Plugin.Misc.NNBoxGenerator;
using Nop.Plugin.Misc.NNBoxGenerator.Data;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.Tasks;
using Nop.Web.Framework.Menu;
using System;
using Nop.Plugin.Misc.NNBoxGenerator.Services;
using Nop.Core.Domain.Common;
using Nop.Services.Shipping.NNBoxGenerator;
using Nop.Services.Shipping;
using Nop.Core.Domain.Orders;
using System.Collections.Generic;
using static Nop.Services.Shipping.GetShippingOptionRequest;
using Nop.Core.Data;
using Newtonsoft.Json;

namespace Nop.Plugin.Misc.NNBoxGenerator
{
    public class NNBoxGeneratorPlugin : BasePlugin, IMiscPlugin, IAdminMenuPlugin, IBoxGeneratorServices
    {
        #region Fields

        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly IWorkContext _workContext;
        private readonly NNBoxGeneratorContext _objectContext;
        private readonly IScheduleTaskService _scheduleTaskService;
        private readonly IBoxPackingService _boxPackingService;

        #endregion

        #region Ctor

        public NNBoxGeneratorPlugin(ISettingService settingService, ILocalizationService localizationService,
            IWebHelper webHelper, IWorkContext workContext, NNBoxGeneratorContext objectContext, 
            IScheduleTaskService scheduleTaskService, IBoxPackingService boxPackingService)
        {
            _settingService = settingService;
            _localizationService = localizationService;
            _webHelper = webHelper;
            _workContext = workContext;
            _objectContext = objectContext;
            _scheduleTaskService = scheduleTaskService;
            _boxPackingService = boxPackingService;
        }

        #endregion

        #region Methods

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new NNBoxGeneratorSettings
            {
                InsuranceSurcharge = decimal.Zero
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("nop.plugin.Misc.NNBoxSelector", "NN Box selector");
            _localizationService.AddOrUpdatePluginLocaleResource("NNop.Plugin.Misc.NNBoxSelector.Fields.Length", "Length");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Width", "Width");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Height", "Height");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Weigth", "Weigth");

            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.VolumenBox", "VolumenBox");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Active", "Active");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Volume", "Volume");

            //database objects
            _objectContext.Install();

            #region Delivery Routes

            var Box1 = new BSBox
            {
                Name = "C-1P",
                Length = 16,
                Width = 12,
                Height = 12,
                WeigthAvailable = Convert.ToDecimal(48.8),
                VolumenBox = 16*12*12,
                Active = true
            };

            _boxPackingService.InsertBox(Box1);

            var Box2 = new BSBox
            {
                Name = "C-3P",
                Length = 18,
                Width = 18,
                Height = 16,
                WeigthAvailable = 48,
                VolumenBox = 18 * 18 * 16,
                Active = true
            };

            _boxPackingService.InsertBox(Box2);

            var Box3 = new BSBox
            {
                Name = "C-4P",
                Length = 24,
                Width = 18,
                Height = 18,
                WeigthAvailable = Convert.ToDecimal(47.6),
                VolumenBox=24*18*18,
                Active = true
            };

            _boxPackingService.InsertBox(Box3);

            var Box4 = new BSBox
            {
                Name = "C-5P",
                Length = 18,
                Width = 18,
                Height = 28,
                WeigthAvailable = 18*18*28,
                Active = true
            };
            _boxPackingService.InsertBox(Box4);

            var Box5 = new BSBox
            {
                Name = "C-8 Cube",
                Length = 8,
                Width = 8,
                Height = 8,
                WeigthAvailable = Convert.ToDecimal(49.5),
                VolumenBox= 8*8*8,
                Active = true
            };
            _boxPackingService.InsertBox(Box5);

            var Box6 = new BSBox
            {
                Name = "C-10 Cube",
                Length = 10,
                Width = 10,
                Height = 10,
                WeigthAvailable = Convert.ToDecimal(49.3),
                VolumenBox=10*10*10,
                Active = true
            };
            _boxPackingService.InsertBox(Box6);

            var Box7 = new BSBox
            {
                Name = "C-12 Cube",
                Length = 12,
                Width = 12,
                Height = 12,
                WeigthAvailable = Convert.ToDecimal(48.9),
                VolumenBox=12*12*12,
                Active = true
            };
            _boxPackingService.InsertBox(Box7);

            var Box8 = new BSBox
            {
                Name = "C-WR18",
                Length = 21,
                Width = 18,
                Height = 44,
                WeigthAvailable = Convert.ToDecimal(114.2),
                VolumenBox= 21*18*44,
                Active = true
            };
            _boxPackingService.InsertBox(Box8);

            var Box9 = new BSBox
            {
                Name = "C-WR24",
                Length = 23,
                Width = 20,
                Height = 44,
                WeigthAvailable = Convert.ToDecimal(113.3),
                VolumenBox= 23*20*44,
                Active = true
            };
            _boxPackingService.InsertBox(Box9);

            var Box10 = new BSBox
            {
                Name = "C-WR24SHORTY",
                Length =24,
                Width =20,
                Height = 34,
                WeigthAvailable = Convert.ToDecimal(114.2),
                VolumenBox= 24*20*34,
                Active = true
            };
            _boxPackingService.InsertBox(Box10);


            #endregion

            base.Install();
        }

        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<NNBoxGeneratorSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("nop.plugin.Misc.NNBoxSelector");
            _localizationService.DeletePluginLocaleResource("NNop.Plugin.Misc.NNBoxSelector.Fields.Length");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Width");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Height");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Weigth");

            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.VolumenBox");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Active");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Misc.NNBoxSelector.Fields.Volume");

            base.Uninstall();
        }

        public override string GetConfigurationPageUrl()
        {
            return _webHelper.GetStoreLocation() + "Admin/NNBoxGenerator/Configure";
        }

        public void ManageSiteMap(SiteMapNode rootNode)
        {
            var pluginMainMenu = new SiteMapNode
            {
                Title = _localizationService.GetResource("Plugins.Misc.NNBoxGeneratorConfig.PluginName", languageId: _workContext.WorkingLanguage.Id, defaultValue: "N&N Box Generator"),
                Visible = false,
                SystemName = NNBoxGeneratorDefaults.MainMenuSystemName,
                IconClass = NNBoxGeneratorDefaults.DefaultIcon
            };

            pluginMainMenu.ChildNodes.Add(new SiteMapNode
            {
                Title = _localizationService.GetResource("Plugins.Misc.NNBoxGeneratorSummary", languageId: _workContext.WorkingLanguage.Id, defaultValue: "Configuration"),
                Url = _webHelper.GetStoreLocation() + "Admin/NNBoxGenerator/Configure",
                Visible = false,
                SystemName = NNBoxGeneratorDefaults.ConfigurationMenuSystemName,
                IconClass = NNBoxGeneratorDefaults.DefaultIcon
            });

            pluginMainMenu.ChildNodes.Add(new SiteMapNode
            {
                Title = _localizationService.GetResource("Plugins.Misc.NNBoxGenerator", languageId: _workContext.WorkingLanguage.Id, defaultValue: "Packing Summary"),
                Url = _webHelper.GetStoreLocation() + "Admin/NNBoxGenerator/PackingSummary",
                Visible = false,
                SystemName = NNBoxGeneratorDefaults.ImporterMenuSystemName,
                IconClass = NNBoxGeneratorDefaults.DefaultIcon
            });

            rootNode.ChildNodes.Add(pluginMainMenu);
        }

        public GetShippingOptionRequest GetBoxesPacking(List<PackageItem> cart, int PackingType, int MarginError)
        {
            return _boxPackingService.GetShippingOptions(cart, PackingType, MarginError);
        }

       
        #endregion
    }
}
