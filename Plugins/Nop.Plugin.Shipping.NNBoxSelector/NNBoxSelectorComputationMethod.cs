using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Shipping.NNBoxSelector.Data;
using Nop.Plugin.Shipping.NNBoxSelector.Models.AlgorithmBase;
using Nop.Plugin.Shipping.NNBoxSelector.Services;
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
using System.Diagnostics;
using System.Linq;

namespace Nop.Plugin.Shipping.NNBoxSelector
{
    public class NNBoxSelectorComputationMethod : BasePlugin, IShippingRateComputationMethod
    {
        #region Fields

        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;
        private readonly ILocalizationService _localizationService;
        private readonly NNBoxselectorSettings _nnDeliverySettings;
        private readonly NNBoxContext _objectContext;
        private readonly IBoxPackingService _boxPackingService;
        private readonly IWorkContext _workContext;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IOrderTotalCalculationService _totalCalculationService;
        
        private readonly ILogger _loggerService;
        private readonly IWebHelper _webHelper;
        private readonly ICompanyService _companyService;
        private  List<ContainerPackingResult> resultFinal = new List<ContainerPackingResult>();


        #endregion

        #region Ctor

        public NNBoxSelectorComputationMethod(IStoreContext storeContext,
            ISettingService settingService,
            ILocalizationService localizationService,
            NNBoxContext objectContext,
            IBoxPackingService boxPackingService,
            IWorkContext workContext,
            IShoppingCartService shoppingCartService,
            IOrderTotalCalculationService totalCalculationService,
           // NNDeliverySettings nnDeliverySettings,
            ILogger loggerService,
            IWebHelper webHelper,
            ICompanyService companyService, NNBoxselectorSettings nnDeliverySettings)
        {
            _storeContext = storeContext;
            _settingService = settingService;
            _localizationService = localizationService;
            _objectContext = objectContext;
            _boxPackingService = boxPackingService;
            _workContext = workContext;
            _shoppingCartService = shoppingCartService;
            _totalCalculationService = totalCalculationService;
            //_nnDeliverySettings = nnDeliverySettings;
            _loggerService = loggerService;
            _webHelper = webHelper;
            _companyService = companyService;
            _nnDeliverySettings = nnDeliverySettings;
        }

        #endregion

        #region Utilities

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/NNBoxSelector/Configure";
        }

        public override void Install()
        {
            //settings
            _settingService.SaveSetting(new NNBoxselectorSettings
            {
                InsuranceSurcharge = decimal.Zero
            });

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("nop.plugin.shipping.nnboxselector", "NN Box selector");
            _localizationService.AddOrUpdatePluginLocaleResource("NNop.Plugin.Shipping.NNBoxSelector.Fields.Length", "Length");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Width", "Width");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Height", "Height");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Weigth", "Weigth");

            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.VolumenBox", "VolumenBox");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Active", "Active");
            _localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Volume", "Volume");
            
            //_localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Available", "Available");
            //_localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Minimum", "Minimum");
            //_localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Name.Hint", "Name");
            //_localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Location.Hint", "Location");
            //_localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Available.Hint", "Available");
            //_localizationService.AddOrUpdatePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Minimum.Hint", "Minimum");

            //database objects
            _objectContext.Install();

            #region Delivery Routes

            //var Box1 = new BSBox
            //{
            //    Name = "Box 8x8x8",
            //    Length = 8,
            //    Width=8,
            //    Height=8,
            //    Weigth=Convert.ToDecimal(49.9),
            //    Active=true
            //};

            //_boxPackingService.InsertBox(Box1);

            //var Box2 = new BSBox
            //{
            //    Name = "Box 10x10x10",
            //    Length = 10,
            //    Width = 10,
            //    Height = 10,
            //    Weigth = Convert.ToDecimal(49.9),
            //    Active = true
            //};

            //_boxPackingService.InsertBox(Box2);

            //var Box3 = new BSBox
            //{
            //    Name = "Box 12x12x12",
            //    Length = 12,
            //    Width = 12,
            //    Height = 12,
            //    Weigth = Convert.ToDecimal(49.9),
            //    Active = true
            //};

            //_boxPackingService.InsertBox(Box3);

            //var Box4 = new BSBox
            //{
            //    Name = "Box Book",
            //    Length = 12,
            //    Width = Convert.ToDecimal(12.5),
            //    Height = Convert.ToDecimal(12.5),
            //    Weigth = Convert.ToDecimal(49.9),
            //    Active = true
            //};
            //_boxPackingService.InsertBox(Box4);

            //var Box5 = new BSBox
            //{
            //    Name = "Box Medium",
            //    Length = 18,
            //    Width = 18,
            //    Height = 16,
            //    Weigth = Convert.ToDecimal(49.9),
            //    Active = true
            //};
            //_boxPackingService.InsertBox(Box5);

            //var Box6 = new BSBox
            //{
            //    Name = "Box Large",
            //    Length = 24,
            //    Width = 18,
            //    Height = 18,
            //    Weigth = Convert.ToDecimal(49.9),
            //    Active = true
            //};
            //_boxPackingService.InsertBox(Box6);

            //var Box7 = new BSBox
            //{
            //    Name = "Box Dish",
            //    Length = 18,
            //    Width = 18,
            //    Height = 28,
            //    Weigth = Convert.ToDecimal(80),
            //    Active = true
            //};
            //_boxPackingService.InsertBox(Box7);

            //var Box8 = new BSBox
            //{
            //    Name = "Box Shorty",
            //    Length = 24,
            //    Width = 20,
            //    Height = 34,
            //    Weigth =120,
            //    Active = true
            //};
            //_boxPackingService.InsertBox(Box8);

            //var Box9 = new BSBox
            //{
            //    Name = "Box WR18",
            //    Length = 21,
            //    Width = 18,
            //    Height = Convert.ToDecimal(44.88),
            //    Weigth = 120,
            //    Active = true
            //};
            //_boxPackingService.InsertBox(Box9);

            //var Box10 = new BSBox
            //{
            //    Name = "Box WR24",
            //    Length = Convert.ToDecimal(23.63),
            //    Width = Convert.ToDecimal(20.63),
            //    Height = Convert.ToDecimal(44.63),
            //    Weigth = 120,
            //    Active = true
            //};
            //_boxPackingService.InsertBox(Box10);


            #endregion

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<NNBoxselectorSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.FixedRate");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.FixedRate.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.DeliveryRoutes");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Description");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.name");

            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Name");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Location");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Available");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Minimum");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Name.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Location.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Available.Hint");
            _localizationService.DeletePluginLocaleResource("Nop.Plugin.Shipping.NNBoxSelector.Fields.Minimum.Hint");

            //database objects
            _objectContext.Uninstall();

            base.Uninstall();
        }

        public decimal? GetFixedRate(GetShippingOptionRequest getShippingOptionRequest)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Public Methods
        public  List<AlgorithmPackingResult> Pack(List<Container> containers, List<Item> items)
        {
           // containers = GetContainers();
            Object sync = new Object { };
            List<ContainerPackingResult> result = new List<ContainerPackingResult>();
            List<AlgorithmPackingResult> containerPackingResultGeneral = new List<AlgorithmPackingResult>();
     
            foreach (var container in containers)
            {
                ContainerPackingResult containerPackingResult = new ContainerPackingResult();
                containerPackingResult.ContainerID = container.ID;

                //    Parallel.ForEach(algorithmTypeIDs, algorithmTypeID =>
                //{

                IBoxSelectorAlgorithm algorithm = GetPackingAlgorithmFromTypeID(1);

                // Until I rewrite the algorithm with no side effects, we need to clone the item list
                // so the parallel updates don't interfere with each other.
                //List<Item> items = new List<Item>();
                //foreach (var item in itemsToPack)
                //{
                //    if (item.Product.ShipSeparately)
                //        ShipAsIs.Add(new Item(item.Product.Id, item.Product.Length, item.Product.Width, item.Product.Height, item.Product.Weight, item.Quantity));
                //    else
                //        items.Add(new Item(item.Product.Id, item.Product.Length, item.Product.Width, item.Product.Height, item.Product.Weight, item.Quantity));
                //}

                //items.Add(new Item(1, Convert.ToDecimal(6.5), 3, 3, Convert.ToDecimal(1.7), 100));

                //itemsToPack.ForEach(item =>
                //{
                //    items.Add(new Item(item.ID, item.Dim1, item.Dim2, item.Dim3, item.Quantity));
                //});

                //Stopwatch stopwatch = new Stopwatch();
                //stopwatch.Start();
                AlgorithmPackingResult algorithmResult = algorithm.Run(container, items);
                //stopwatch.Stop();

                //algorithmResult.PackTimeInMilliseconds = stopwatch.ElapsedMilliseconds;

                decimal containerVolume = container.Length * container.Width * container.Height;
                decimal itemVolumePacked = algorithmResult.PackedItems.Sum(i => i.Volume);
                decimal itemWeightPacked = algorithmResult.PackedItems.Sum(i => i.Quantity * i.WeightPacked);
                decimal itemVolumeUnpacked = algorithmResult.UnpackedItems.Sum(i => i.Volume);

                algorithmResult.PercentContainerVolumePacked = Math.Round(itemVolumePacked / containerVolume * 100, 2);
                algorithmResult.PercentItemVolumePacked = Math.Round(itemVolumePacked / (itemVolumePacked + itemVolumeUnpacked) * 100, 2);
                algorithmResult.PercentItemWeightPacked = itemWeightPacked;
                algorithmResult.TotalVolumePacked = itemVolumePacked;
                //foreach (var item in algorithmResult.PackedItems)
                //{
                //    item.WeightPacked = item.Quantity*
                //}

                lock (sync)
                {
                    containerPackingResult.AlgorithmPackingResults.Add(algorithmResult);
                    containerPackingResultGeneral.Add(algorithmResult);
                }
                //});

                containerPackingResult.AlgorithmPackingResults = containerPackingResult.AlgorithmPackingResults.OrderBy(r => r.AlgorithmName).ToList();

                lock (sync)
                {
                    result.Add(containerPackingResult);
                }
            }

            return containerPackingResultGeneral;
        }

        public GetShippingOptionResponse GetShippingOptions(GetShippingOptionRequest getShippingOptionRequest)
        {
            var response = new GetShippingOptionResponse();
            //List<AlgorithmPackingResult> containerPackingResultGeneral = new List<AlgorithmPackingResult>();

            var itemsToPack = GetShippingOptionsCart(getShippingOptionRequest);
            List<Item> ShipAsIs = new List<Item>();

            List<Item> items = new List<Item>();
            // items.Add(new Item(1, Convert.ToDecimal(6.5), 3, 3, Convert.ToDecimal(1.7), 100));

            items.Add(new Item(1, Convert.ToDecimal(9), 33, 19, Convert.ToDecimal(20), 3, "Product 1"));
            items.Add(new Item(1, Convert.ToDecimal(3), 9, 3, Convert.ToDecimal(0.76), 2, "Product 2"));
            //items.Add(new Item(1, Convert.ToDecimal(4.5), 9, 9, Convert.ToDecimal(1.2), 1));
            //items.Add(new Item(1, Convert.ToDecimal(3), 9, 8, Convert.ToDecimal(1), 1));
            //items.Add(new Item(1, Convert.ToDecimal(7), 6, 3, Convert.ToDecimal(2), 4));
            //items.Add(new Item(1, Convert.ToDecimal(4), 12, 8, Convert.ToDecimal(3.6), 1));

            var itemsOrder = items.OrderByDescending(r => r.Volume).ToList();

            List<Container> containers = GetContainers(itemsOrder).OrderBy(r=>r.Volume).ToList();


            //   items.Sort();

            var containerPackingResultGeneral = Pack(containers, itemsOrder);
            int pack = itemsOrder.Count;
            do
            {
                var PercentContainerVolumePacked2 = containerPackingResultGeneral.OrderByDescending(r => r.PercentContainerVolumePacked).Take(3).ToList();
                var PercentContainerVolumePacked3 = PercentContainerVolumePacked2.OrderByDescending(r => r.PackedItems.Count).Take(1).ToList();

                ContainerPackingResult containerPackingResultFinal = new ContainerPackingResult();
                containerPackingResultFinal.ContainerID = 1;
                containerPackingResultFinal.AlgorithmPackingResults = PercentContainerVolumePacked3;
                resultFinal.Add(containerPackingResultFinal);

                var unpackedItems = PercentContainerVolumePacked3.SelectMany(r => r.UnpackedItems).ToList();
                pack = unpackedItems.Count;

                if (unpackedItems.Count > 0)
                {
                    var itemsToPackPending = unpackedItems.ToList();
                    var containerList = containers = GetContainers(itemsToPackPending); 
                    containerPackingResultGeneral = Pack(containerList, itemsToPackPending);

                }

            } while (pack != 0);

            //response.ShippingOptions.Add(new Core.Domain.Shipping.ShippingOption
            //{
            //    Description = string.Empty,
            //    Name = _localizationService.GetResource("Nop.Plugin.Shipping.NNBoxSelector.Name"),
            //    Rate = _nnDeliverySettings.InsuranceSurcharge,
            //    ShippingRateComputationMethodSystemName = "Shipping.NNBoxSelector",
            //    resultFinal = resultFinal,
            //    Disable = false
            //});

            return response;
        }

        #endregion

        #region Private Region

        /// <summary>
        /// Sorts two dimensional array type of string,
        /// by selected culumns, in selected sort order.
        /// Selection Sort Algorithm is implemented
        /// for sorting values inside array.
        /// </summary>
        /// <param name="array">
        /// Two dimensional array, type of string,
        /// contaning values that are going to be sorted.
        /// </param>
        /// <param name="sort_directive">
        /// Two dimensional array, type of int.
        /// First column contains the order of the column indexes
        /// by which the array will be sorted.
        /// Second column contains column sort order.
        /// Sort order for each column is defined by ineger values :
        /// -1 - sort column values in Descending sort order
        ///  0 - column is not going to be sorted
        ///  1 - sort column values in Ascending sort order
        /// </param>
        /// <returns>
        /// Sorted array by selected columns in selected sort order.
        /// </returns>
        public string[,] Sort_String(string[,] array, int[,] sort_directive)
        {
            // number of rows iside array
            int array_rows = array.GetLength(0);
            // number of columns inside array
            int array_columns = array.Length / array_rows;
            // number of columns to be sorted
            int sort_directive_columns = sort_directive.GetLength(0);
            //
            for (int i = 0; i < array_rows - 1; i++)
            {
                for (int j = i + 1; j < array_rows; j++)
                {
                    for (int c = 0; c < sort_directive_columns; c++)
                    {
                        //
                        // sort array values in descending sort order
                        //
                        if (sort_directive[c, 1] == -1 &&
                           array[i, sort_directive[c, 0]].CompareTo(array[j, sort_directive[c, 0]]) < 0)
                        {
                            //
                            // if values are in ascending sort order
                            // swap values
                            //
                            for (int d = 0; d < array_columns; d++)
                            {
                                string h = array[j, d];
                                array[j, d] = array[i, d];
                                array[i, d] = h;
                            }

                            break;
                        }
                        //
                        // if values are in correct sort order break
                        //
                        else if (sort_directive[c, 1] == -1 &&
                                array[i, sort_directive[c, 0]].CompareTo(array[j, sort_directive[c, 0]]) > 0)
                            break;
                        //
                        // sort array values in ascending sort order
                        //
                        else if (sort_directive[c, 1] == 1 &&
                                array[i, sort_directive[c, 0]].CompareTo(array[j, sort_directive[c, 0]]) > 0)
                        {
                            //
                            // if values are in descending sort order
                            // swap values
                            //
                            for (int d = 0; d < array_columns; d++)
                            {
                                string h = array[j, d];
                                array[j, d] = array[i, d];
                                array[i, d] = h;
                            }

                            break;
                        }
                        //
                        // if values are in correct sort order break
                        //
                        else if (sort_directive[c, 1] == 1 &&
                                array[i, sort_directive[c, 0]].CompareTo(array[j, sort_directive[c, 0]]) < 0)
                            break;
                        //
                        // if values are equal
                        // select next sort directive
                        //
                    }
                }
            }

            return array;
        }

        public List<Container> GetContainers(List<Item> items)
        {
            var DimHiger = items.FirstOrDefault();
            var MaxVal1 = DimHiger.Dim1;
            var MaxVal2 = DimHiger.Dim2;
            var MaxVal3 = DimHiger.Dim3;

            var BoxesList1 = _boxPackingService.ListBoxes().Where(r=>r.Height>= MaxVal1 && r.Length>= MaxVal2 && r.Width>= MaxVal3);
            var BoxesList2 = _boxPackingService.ListBoxes().Where(r => r.Height >= MaxVal1 && r.Length >= MaxVal3 && r.Width >= MaxVal2);

            var BoxesList3 = _boxPackingService.ListBoxes().Where(r => r.Height >= MaxVal2 && r.Length >= MaxVal1 && r.Width >= MaxVal3);
            var BoxesList4 = _boxPackingService.ListBoxes().Where(r => r.Height >= MaxVal2 && r.Length >= MaxVal3 && r.Width >= MaxVal1);

            var BoxesList5 = _boxPackingService.ListBoxes().Where(r => r.Height >= MaxVal3 && r.Length >= MaxVal2 && r.Width >= MaxVal1);
            var BoxesList6 = _boxPackingService.ListBoxes().Where(r => r.Height >= MaxVal3 && r.Length >= MaxVal1 && r.Width >= MaxVal2);

            List<Container> container = new List<Container>();

            if (BoxesList1.Count()>0)
            {
                foreach (var item in BoxesList1)
                {
                    var ValidContainer = container.Where(r => r.ID == item.Id);
                    if(ValidContainer.Count() == 0)
                        container.Add(new Container( item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name));
                }
            }
            if (BoxesList2.Count() > 0)
            {
                foreach (var item in BoxesList2)
                {
                    var ValidContainer = container.Where(r => r.ID == item.Id);
                    if (ValidContainer.Count() == 0)
                        container.Add(new Container( item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name));
                }
            }

            if (BoxesList3.Count() > 0)
            {
                foreach (var item in BoxesList3)
                {
                    var ValidContainer = container.Where(r => r.ID == item.Id);
                    if (ValidContainer.Count() == 0)
                        container.Add(new Container( item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name));
                }
            }
            if (BoxesList4.Count() > 0)
            {
                foreach (var item in BoxesList4)
                {
                    var ValidContainer = container.Where(r => r.ID == item.Id);
                    if (ValidContainer.Count() == 0)
                        container.Add(new Container( item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name));
                }
            }
            if (BoxesList5.Count() > 0)
            {
                foreach (var item in BoxesList5)
                {
                    var ValidContainer = container.Where(r => r.ID == item.Id);
                    if (ValidContainer.Count() == 0)
                        container.Add(new Container(item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name));
                }
            }
            if (BoxesList6.Count() > 0)
            {
                foreach (var item in BoxesList6)
                {
                    var ValidContainer = container.Where(r => r.ID == item.Id);
                    if (ValidContainer.Count() == 0)
                        container.Add(new Container( item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name));
                }
            }
            container = container.OrderByDescending(r => r.Volume).ToList();

            return container;
        }

        private static List<ShoppingCartItem> GetShippingOptionsCart(GetShippingOptionRequest ShippingOptionRequest)
        {
            var cart = ShippingOptionRequest.Items.Select(item =>
            {
                var shoppingCartItem = item.ShoppingCartItem;
                shoppingCartItem.Quantity = item.GetQuantity();
                return shoppingCartItem;
            }).ToList();

            return cart;
        }

        /// <summary>
		/// Gets the packing algorithm from the specified algorithm type ID.
		/// </summary>
		/// <param name="algorithmTypeID">The algorithm type ID.</param>
		/// <returns>An instance of a packing algorithm implementing AlgorithmBase.</returns>
		/// <exception cref="System.Exception">Invalid algorithm type.</exception>
		private static IBoxSelectorAlgorithm GetPackingAlgorithmFromTypeID(int algorithmTypeID)
        {
            switch (algorithmTypeID)
            {
                case (int)AlgorithmType.EB_AFIT:
                    return new BoxSelectorAlgorithm();

                default:
                    throw new Exception("Invalid algorithm type.");
            }
        }

        #endregion

        #region Properties

        public ShippingRateComputationMethodType ShippingRateComputationMethodType => ShippingRateComputationMethodType.Offline;

        public IShipmentTracker ShipmentTracker => null;

        #endregion
    }
}