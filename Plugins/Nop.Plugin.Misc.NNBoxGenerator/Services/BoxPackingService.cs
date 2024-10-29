using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Plugin.Misc.NNBoxGenerator.Models.AlgorithmBase;
using Nop.Services.Localization;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Tracking;
using System;
using System.Collections.Generic;
using System.Linq;
using static Nop.Services.Shipping.GetShippingOptionRequest;
using BSBox = Nop.Core.Domain.Common.BSBox;

namespace Nop.Plugin.Misc.NNBoxGenerator.Services
{
    public class BoxPackingService : IBoxPackingService
    {
        #region Fields

        private readonly IRepository<BSBox> _repository;
        private readonly ILocalizationService _localizationService;
        private readonly NNBoxGeneratorSettings _nnDeliverySettings;
        private readonly IRepository<BSItemPackedBox> _itemPackedBox;
        private readonly IRepository<BSPackedOrder> _packedOrder;
        private readonly IRepository<BSPackedOrder> _PackedBox;
        private readonly IRepository<BSItemPack> _BSItemPack;

        private List<ContainerPackingResult> resultFinal = new List<ContainerPackingResult>();

        #endregion

        #region Ctor

        public BoxPackingService(IRepository<BSBox> repository,
            ILocalizationService localizationService,
            NNBoxGeneratorSettings nnDeliverySettings, IRepository<BSItemPackedBox> itemPackedBox,
             IRepository<BSPackedOrder> packedOrder, IRepository<BSPackedOrder> PackedBox, IRepository<BSItemPack> BSItemPack)
        {
            _repository = repository;
            _localizationService = localizationService;
            _nnDeliverySettings = nnDeliverySettings;
            _itemPackedBox = itemPackedBox;
            _packedOrder = packedOrder;
            _PackedBox = PackedBox;
            _BSItemPack = BSItemPack;
        }

        #endregion

        #region BSBox

        public void InsertBox(BSBox box)
        {
            if (box == null)
                throw new ArgumentNullException(nameof(box));

            _repository.Insert(box);
        }
        public IPagedList<BSBox> SearchBoxes(string name, string location, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var deliveryRoutes = _repository.Table;

            if (!string.IsNullOrEmpty(name))
                deliveryRoutes = deliveryRoutes.Where(d => d.Name.Contains(name));


            return new PagedList<BSBox>(deliveryRoutes, pageIndex, pageSize);
        }
        public BSBox GetById(int id)
        {
            if (id == 0)
                return null;

            return _repository.GetById(id);
        }
        public void UpdateBSBox(BSBox BSBox)
        {
            if (BSBox == null)
                throw new ArgumentNullException(nameof(BSBox));

            _repository.Update(BSBox);
        }
        public void DeleteBoxes(BSBox BSBox)
        {
            if (BSBox == null)
                throw new ArgumentNullException(nameof(BSBox));

            _repository.Delete(BSBox);
        }
        public List<BSBox> ListBoxes()
        {
            return _repository.Table.ToList();
        }

        #endregion

        #region  AlgorithmPackingResult
        public GetShippingOptionRequest GetShippingOptions(List<PackageItem> cart, int PackingType, int MarginError)
        {
            var response = new GetShippingOptionRequest();

            var itemsToPack = cart;

            List<Item> ShipAsIs = new List<Item>();

            List<Item> items = new List<Item>();

            foreach (var item in cart)
            {
                if (!item.ShoppingCartItem.Product.UnShippable) {
                    if (!item.ShoppingCartItem.Product.ShipSeparately)
                    {						
                        for (int i = 0; i < item.ShoppingCartItem.Quantity; i++)
                        {
                                items.Add(new Item(item.ShoppingCartItem.ProductId, item.ShoppingCartItem.Product.Height, item.ShoppingCartItem.Product.Length, item.ShoppingCartItem.Product.Width, item.ShoppingCartItem.Product.Weight, 1, item.ShoppingCartItem.Product.Name));
                        }
					}					
                }               
            }           
            // items.Add(new Item(1, Convert.ToDecimal(6.5), 3, 3, Convert.ToDecimal(1.7), 100));
            //items.Add(new Item(2388, Convert.ToDecimal(9), 33, 19, Convert.ToDecimal(20), 3, "Product 1"));
            //items.Add(new Item(2389, Convert.ToDecimal(3), 9, 3, Convert.ToDecimal(0.76), 2, "Product 2"));
            //items.Add(new Item(1, Convert.ToDecimal(4.5), 9, 9, Convert.ToDecimal(1.2), 1));
            //items.Add(new Item(1, Convert.ToDecimal(3), 9, 8, Convert.ToDecimal(1), 1));
            //items.Add(new Item(1, Convert.ToDecimal(7), 6, 3, Convert.ToDecimal(2), 4));
            //items.Add(new Item(1, Convert.ToDecimal(4), 12, 8, Convert.ToDecimal(3.6), 1));

            var itemsOrder = items.OrderByDescending(r => r.Volume).ToList();

            List<Container> containers = GetContainers(itemsOrder).OrderBy(r => r.Volume).ToList();

            List<AlgorithmPackingResult> containerPackingResultGeneral = new List<AlgorithmPackingResult>();

            if(PackingType==1)
                containerPackingResultGeneral = PackVolumen(containers, itemsOrder, MarginError);
            else
                containerPackingResultGeneral = PackDimension(containers, itemsOrder);

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

                    if (PackingType == 1)
                        containerPackingResultGeneral = PackVolumen(containers, itemsToPackPending, MarginError);
                    else
                        containerPackingResultGeneral = PackDimension(containerList, itemsToPackPending);

                }

            } while (pack != 0);

            response.ResultFinal = resultFinal;

            return response;
        }
        public List<AlgorithmPackingResult> PackDimension(List<Container> containers, List<Item> items)
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

                algorithmResult.ContainerName = container.Name;
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

        private List<AlgorithmPackingResult> PackVolumen(List<Container> containers, List<Item> items, int MarginError)
        {
            Object sync = new Object { };
            List<ContainerPackingResult> result = new List<ContainerPackingResult>();
            List<AlgorithmPackingResult> containerPackingResultGeneral = new List<AlgorithmPackingResult>();

            decimal VolumenTotalItems = items.Sum(r => r.Volume * r.Quantity);

            foreach (var container in containers)
            {
                //if(VolumenTotalItems!=0)
                    
                //Volumen Box
                var itemPerBox = Math.Floor(container.Volume / VolumenTotalItems);
                var MarginErrorBox = Math.Floor((itemPerBox * MarginError) / 100);
                var ItemsPerBoxError = itemPerBox - MarginErrorBox;

                // Margin Error Container 
                container.Volume = container.Volume - ((container.Volume * MarginError) / 100);

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
                AlgorithmPackingResult algorithmResult = algorithm.RunVolumen(container, items);
                //stopwatch.Stop();

                //algorithmResult.PackTimeInMilliseconds = stopwatch.ElapsedMilliseconds;

                decimal containerVolume = container.Length * container.Width * container.Height;
                decimal itemVolumePacked = algorithmResult.PackedItems.Sum(i => i.Volume);
                decimal itemWeightPacked = algorithmResult.PackedItems.Sum(i => i.Quantity * i.WeightPacked);
                decimal itemVolumeUnpacked = algorithmResult.UnpackedItems.Sum(i => i.Volume);

                algorithmResult.PercentContainerVolumePacked = Math.Round(itemVolumePacked / containerVolume * 100, 2);
                algorithmResult.PercentItemVolumePacked = Math.Round(itemVolumePacked / (itemVolumePacked + itemVolumeUnpacked) * 100, 2);
                algorithmResult.PercentItemWeightPacked = itemWeightPacked+container.WeightBox;
                algorithmResult.TotalVolumePacked = itemVolumePacked;

                algorithmResult.ContainerName = container.Name;
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

        //public IPagedList<AlgorithmPackingResult> SearchPacking(string name, int OrderId, int pageIndex = 0, int pageSize = int.MaxValue)
        //{
        //    var LastItemPackedBox = _itemPackedBox.Table.ToList();

        //    if (!string.IsNullOrEmpty(name))
        //        LastItemPackedBox = LastItemPackedBox.Where(r => r.CustomerId.ToString() == name).OrderByDescending(r => r.Id).ToList();

        //    if (OrderId!=0)
        //        LastItemPackedBox = LastItemPackedBox.Where(r => r.OrderId == OrderId).OrderByDescending(r => r.Id).ToList();

        //    List<AlgorithmPackingResult> ListItems = new List<AlgorithmPackingResult>();

        //    if (LastItemPackedBox.Count > 0)
        //    {
        //        foreach (var item in LastItemPackedBox)
        //        {
        //            var box = _repository.GetById(item.ContainerId);
        //            var ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(item.ContainerPackingResult);

        //            if (box != null)
        //            {
        //                var container = new Container(

        //                    box.Length,
        //                    box.Width,
        //                    box.Height,
        //                    box.WeigthAvailable,
        //                    box.Id,
        //                    box.Name
        //                );
        //                ContainerPackingResult.Container = container;
        //                ContainerPackingResult.Id = item.Id;
        //                ContainerPackingResult.Active = item.Active;
        //            }
        //            ListItems.Add(ContainerPackingResult);
        //        }
        //    }

        //    return new PagedList<AlgorithmPackingResult>(ListItems, pageIndex, pageSize);
        //}

        public IPagedList<BSItemPackedBox> ListPacking(string name, int OrderId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var LastItemPackedBox = _itemPackedBox.Table.ToList();

            if (!string.IsNullOrEmpty(name))
                LastItemPackedBox = LastItemPackedBox.Where(r => r.CustomerId.ToString() == name).OrderByDescending(r => r.Id).ToList();

            if (OrderId != 0)
                LastItemPackedBox = LastItemPackedBox.Where(r => r.OrderId == OrderId).OrderByDescending(r => r.Id).ToList();

            return new PagedList<BSItemPackedBox>(LastItemPackedBox, pageIndex, pageSize);
        }

    public AlgorithmPackingResult PackingById(int id)
        {
            var LastItemPackedBox = _itemPackedBox.Table.Where(r => r.Id == id).FirstOrDefault();

            AlgorithmPackingResult ListItems = new AlgorithmPackingResult();

            if (LastItemPackedBox != null)
            {
                
                if (LastItemPackedBox.IsAsShip)
                {
                    var box = _repository.GetById(LastItemPackedBox.ContainerId);
                    AlgorithmPackingResult ContainerPackingResult = new AlgorithmPackingResult();

                    if (LastItemPackedBox.ContainerPackingResult != "own")
                    {
                        ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(LastItemPackedBox.ContainerPackingResult);
                    }
                    var ContainerShippingOptions = JsonConvert.DeserializeObject<List<ShippingOption>>(LastItemPackedBox.GetShippingOptionResponse);

                    if (box == null)
                    {
                        var container = new Container(

                            LastItemPackedBox.FinalLength,
                            LastItemPackedBox.FinalWidth,
                            LastItemPackedBox.FinalHeight,
                            LastItemPackedBox.WeightTotalBox,
                            0,
                            "own",
                            0
                        );
                        ContainerPackingResult.Container = container;
                        ContainerPackingResult.Id = LastItemPackedBox.Id;
                        ContainerPackingResult.Active = LastItemPackedBox.Active;
                        ContainerPackingResult.TotalVolumePacked = LastItemPackedBox.VolumenTotalBox;
                        ContainerPackingResult.PercentItemWeightPacked = LastItemPackedBox.WeightTotalBox;
                        ContainerPackingResult.IsAsShip = LastItemPackedBox.IsAsShip;


                    }
                    ListItems = ContainerPackingResult;

                    ListItems.ShippingOption = ContainerShippingOptions;
                }
                else
                {
                    var box = _repository.GetById(LastItemPackedBox.ContainerId);
                    var ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(LastItemPackedBox.ContainerPackingResult);
                    var ContainerShippingOptions = JsonConvert.DeserializeObject<List<ShippingOption>>(LastItemPackedBox.GetShippingOptionResponse);

                    if (box != null)
                    {
                        var container = new Container(

                            box.Length,
                            box.Width,
                            box.Height,
                            box.WeigthAvailable,
                            box.Id,
                            box.Name,
                            box.WeigthBox

                        );
                        ContainerPackingResult.Container = container;
                        ContainerPackingResult.Id = LastItemPackedBox.Id;
                        ContainerPackingResult.Active = LastItemPackedBox.Active;
                    }
                    ListItems = ContainerPackingResult;

                    ListItems.ShippingOption = ContainerShippingOptions;
                }
                
            }

            return ListItems;
        }

        public AlgorithmPackingResult PackingOrderById(int id)
        {
            var LastItemPackedBox = _packedOrder.Table.Where(r => r.Id == id).FirstOrDefault();

            AlgorithmPackingResult ListItems = new AlgorithmPackingResult();

            if (LastItemPackedBox != null)
            {
                    var ContainerShippingOptions = JsonConvert.DeserializeObject<List<ShippingOption>>(LastItemPackedBox.ShippingOptions);

                   
                    ListItems.ShippingOption = ContainerShippingOptions;
                

            }

            return ListItems;
        }

        public List<AlgorithmPackingResult> GetBoxByOrder()
        {
            var LastItemPackedBox = _itemPackedBox.Table.OrderByDescending(r => r.Id).ToList();
            List<AlgorithmPackingResult> ListItems = new List<AlgorithmPackingResult>();

            if (LastItemPackedBox.Count > 0)
            {
                foreach (var item in LastItemPackedBox)
                {
                    var box = _repository.GetById(item.ContainerId);
                    var ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(item.ContainerPackingResult);

                    if (box != null)
                    {
                        var container = new Container(
                            box.Length,
                            box.Width,
                            box.Height,
                            box.WeigthAvailable,
                            box.Id,
                            box.Name,
                            box.WeigthBox
                        );
                        ContainerPackingResult.Container = container;
                        ContainerPackingResult.Id = item.Id;
                    }
                    ListItems.Add(ContainerPackingResult);
                }
            }

            return ListItems;
        }
    
    #endregion

        #region BSItemPackedBox

        public BSItemPackedBox GetItemPackedBoxId(int id)
        {
            var LastItemPackedBox = _itemPackedBox.Table.Where(r => r.Id == id).FirstOrDefault();

            return LastItemPackedBox;
        }

        public List<BSItemPackedBox> GetItemPackedBoxIdList(int id)
        {
            var LastItemPackedBox = _itemPackedBox.Table.Where(r => r.CustomerId == id).ToList();

            return LastItemPackedBox;
        }

        public List<BSItemPackedBox> GetBSItemPackedBoxList(int id)
        {
            var LastItemPackedBox = _itemPackedBox.Table.Where(r => r.BSPackedOrderId == id).ToList();

            return LastItemPackedBox;
        }

        public void UpdateBSItemPackedBox(BSItemPackedBox ItemPackedBox)
        {
            if (ItemPackedBox == null)
                throw new ArgumentNullException(nameof(ItemPackedBox));

            _itemPackedBox.Update(ItemPackedBox);
        }


        #endregion

        public List<BSItemPack> GetBSItemPackList(int id)
        {
            var LastItemPackedBox = _BSItemPack.Table.Where(r => r.BsItemPackBoxId == id).ToList();

            return LastItemPackedBox;
        }

        #region BSPackedOrder
        public BSPackedOrder GetBSPackedOrderById(int id)
        {
            var LastItemPackedBox = _packedOrder.Table.Where(r => r.Id == id).FirstOrDefault();

            return LastItemPackedBox;
        }

        public void UpdateBSPackedOrder(BSPackedOrder ItemPackedBox)
        {
            if (ItemPackedBox == null)
                throw new ArgumentNullException(nameof(ItemPackedBox));

            _packedOrder.Update(ItemPackedBox);
        }
        public List<BSPackedOrder> BSPackedOrderById(int id)
        {
            var LastPackedOrder = _packedOrder.Table.Where(r => r.Id == id).ToList();

            return LastPackedOrder;
        }

        public List<BSPackedOrder> GetOrderSummaryList()
        {

            var LastPackedOrder = _PackedBox.Table.ToList();

            return LastPackedOrder;
        }

        public IPagedList<BSPackedOrder> SearchPacking(string name, int OrderId, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var LastItemPackedBox = _packedOrder.Table.OrderByDescending(r=>r.DateUpdated).ToList();

            if (!string.IsNullOrEmpty(name))
                LastItemPackedBox = LastItemPackedBox.Where(r => r.CustomerId.ToString() == name).OrderByDescending(r => r.Id).ToList();

            if (OrderId != 0)
                LastItemPackedBox = LastItemPackedBox.Where(r => r.OrderId == OrderId).OrderByDescending(r => r.Id).ToList();

            return new PagedList<BSPackedOrder>(LastItemPackedBox, pageIndex, pageSize);
        }

        #endregion

        #region Other Methods

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
            List<Container> container = new List<Container>();

            if (DimHiger != null)
            {
                var MaxVal1 = DimHiger.Dim1;
                var MaxVal2 = DimHiger.Dim2;
                var MaxVal3 = DimHiger.Dim3;

                var BoxesList1 = ListBoxes().Where(r => r.Height >= MaxVal1 && r.Length >= MaxVal2 && r.Width >= MaxVal3);
                var BoxesList2 = ListBoxes().Where(r => r.Height >= MaxVal1 && r.Length >= MaxVal3 && r.Width >= MaxVal2);

                var BoxesList3 = ListBoxes().Where(r => r.Height >= MaxVal2 && r.Length >= MaxVal1 && r.Width >= MaxVal3);
                var BoxesList4 = ListBoxes().Where(r => r.Height >= MaxVal2 && r.Length >= MaxVal3 && r.Width >= MaxVal1);

                var BoxesList5 = ListBoxes().Where(r => r.Height >= MaxVal3 && r.Length >= MaxVal2 && r.Width >= MaxVal1);
                var BoxesList6 = ListBoxes().Where(r => r.Height >= MaxVal3 && r.Length >= MaxVal1 && r.Width >= MaxVal2);


                if (BoxesList1.Count() > 0)
                {
                    foreach (var item in BoxesList1)
                    {
                        var ValidContainer = container.Where(r => r.ID == item.Id);
                        if (ValidContainer.Count() == 0)
                            container.Add(new Container(item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name, item.WeigthBox));
                    }
                }
                if (BoxesList2.Count() > 0)
                {
                    foreach (var item in BoxesList2)
                    {
                        var ValidContainer = container.Where(r => r.ID == item.Id);
                        if (ValidContainer.Count() == 0)
                            container.Add(new Container(item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name, item.WeigthBox));
                    }
                }

                if (BoxesList3.Count() > 0)
                {
                    foreach (var item in BoxesList3)
                    {
                        var ValidContainer = container.Where(r => r.ID == item.Id);
                        if (ValidContainer.Count() == 0)
                            container.Add(new Container(item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name, item.WeigthBox));
                    }
                }
                if (BoxesList4.Count() > 0)
                {
                    foreach (var item in BoxesList4)
                    {
                        var ValidContainer = container.Where(r => r.ID == item.Id);
                        if (ValidContainer.Count() == 0)
                            container.Add(new Container(item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name, item.WeigthBox));
                    }
                }
                if (BoxesList5.Count() > 0)
                {
                    foreach (var item in BoxesList5)
                    {
                        var ValidContainer = container.Where(r => r.ID == item.Id);
                        if (ValidContainer.Count() == 0)
                            container.Add(new Container(item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name, item.WeigthBox));
                    }
                }
                if (BoxesList6.Count() > 0)
                {
                    foreach (var item in BoxesList6)
                    {
                        var ValidContainer = container.Where(r => r.ID == item.Id);
                        if (ValidContainer.Count() == 0)
                            container.Add(new Container(item.Length, item.Width, item.Height, item.WeigthAvailable, item.Id, item.Name, item.WeigthBox));
                    }
                }
                container = container.OrderByDescending(r => r.Volume).ToList();
            }
            return container;
        }

        //private static List<ShoppingCartItem> GetShippingOptionsCart(GetShippingOptionRequest ShippingOptionRequest)
        //{
        //    var cart = ShippingOptionRequest.Items.Select(item =>
        //    {
        //        var shoppingCartItem = item.ShoppingCartItem;
        //        shoppingCartItem.Quantity = item.GetQuantity();
        //        return shoppingCartItem;
        //    }).ToList();

        //    return cart;
        //}

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
