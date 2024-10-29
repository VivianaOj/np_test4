using Newtonsoft.Json;
using Nop.Core.Data;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Catalog;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Shipping.NNBoxGenerator
{
    public partial class BoxesGeneratorServices : IBoxesGeneratorServices
    {
        private readonly IRepository<BSItemPackedBox> _repository;
        private readonly IRepository<BSBox> _bsBox;
        private readonly IRepository<BSItemPack> _itemPack;
        private readonly IRepository<BSPackedOrder> _packedOrder;

        private readonly IProductService _productServices;
        private readonly ILogger _logger;

        #region Ctor

        public BoxesGeneratorServices(IRepository<BSItemPackedBox> repository, 
            IRepository<BSBox> bsBox, IRepository<BSItemPack> itemPack, IRepository<BSPackedOrder> packedOrder ,IProductService productServices, ILogger logger)
        {
            _repository = repository;
            _bsBox = bsBox;
            _itemPack = itemPack;
            _productServices = productServices;
            _logger = logger;
            _packedOrder=packedOrder;
    }

        #endregion

        public void InsertBoxGenerator(GetShippingOptionResponse GetShippingOptions, Customer customer, bool isAsShip, GetShippingOptionRequest items)
        {
            var LastPackedOrder = _packedOrder.Table.Where(x => x.CustomerId == customer.Id && !x.Active).FirstOrDefault();
            int IdPackedOrder = 0;

            if (LastPackedOrder != null)
            {
                //LastPackedOrder.OrderId = 0;
                //LastPackedOrder.DateCreated = DateTime.Now;
                LastPackedOrder.DateUpdated = DateTime.Now;
                //LastPackedOrder.Active = false;
                //LastPackedOrder.CustomerId = customer.Id;
                //LastPackedOrder.Customer = customer;
                LastPackedOrder.ShippingOptions = JsonConvert.SerializeObject(GetShippingOptions.ShippingOptions);
                IdPackedOrder = LastPackedOrder.Id;
                LastPackedOrder.IsCommercial = GetShippingOptions.IsCommercial;
                _packedOrder.Update(LastPackedOrder);

            }
            else
            {
                BSPackedOrder BSPackedOrder = new BSPackedOrder();
                BSPackedOrder.OrderId = 0;
                BSPackedOrder.DateCreated = DateTime.Now;
                BSPackedOrder.DateUpdated = DateTime.Now;
                BSPackedOrder.Active = false;
                BSPackedOrder.CustomerId = customer.Id;
                BSPackedOrder.Customer = customer;
                BSPackedOrder.ShippingOptions = JsonConvert.SerializeObject(GetShippingOptions.ShippingOptions);
                BSPackedOrder.IsCommercial = GetShippingOptions.IsCommercial;

                _packedOrder.Insert(BSPackedOrder);
                IdPackedOrder = BSPackedOrder.Id;
            }

            if (isAsShip)
            {
                //var LastItemPackedBoxOwm = _repository.Table.Where(x => x.CustomerId == customer.Id && !x.Active && x.IsAsShip).ToList();
                //if (LastItemPackedBoxOwm.Count > 0)
                //{
                //    foreach (var item in LastItemPackedBoxOwm)
                //    {
                //        foreach (var x in items.Items)
                //        {
                //            var products = _itemPack.Table.Where(r => r.BsItemPackBoxId == item.Id && r.ProductId == x.ShoppingCartItem.ProductId);

                //            if (products.Count() > 0)
                //                _repository.Delete(item);
                //        }
                //    }
                //}

                var package = items.Items.Where(r => r.ShoppingCartItem.Product.IncrementQuantity > 1).ToList();

                foreach (var item in items.Items)
                {                        
                    if (item.ShoppingCartItem.Product.IncrementQuantity > 1 && item.ShoppingCartItem.Quantity > 1)
                    {
                       var quantity = item.ShoppingCartItem.Quantity / item.ShoppingCartItem.Product.IncrementQuantity;

                        for (int i = 0; i < quantity; i++)
                        {
                            BSItemPackedBox bSItemPackedBox = new BSItemPackedBox();
                            bSItemPackedBox.PercentBoxVolumePacked = 100;
                            bSItemPackedBox.PackTime = 0;
                            bSItemPackedBox.ItemsPacked = 1;
                            bSItemPackedBox.WeightTotalBox = item.ShoppingCartItem.Product.Weight;
                            bSItemPackedBox.VolumenTotalBox = item.ShoppingCartItem.Product.Height * item.ShoppingCartItem.Product.Width * item.ShoppingCartItem.Product.Length;
                            bSItemPackedBox.GetShippingOptionResponse = JsonConvert.SerializeObject(GetShippingOptions.ShippingOptions);
                            bSItemPackedBox.ContainerPackingResult = "own";
                            bSItemPackedBox.CustomerId = customer.Id;
                            bSItemPackedBox.Customer = customer;
                            bSItemPackedBox.ContainerId = 0;
                            bSItemPackedBox.Active = false;
                            bSItemPackedBox.ContainerName = "own";
                            bSItemPackedBox.FinalHeight = item.ShoppingCartItem.Product.Height;
                            bSItemPackedBox.FinalLength = item.ShoppingCartItem.Product.Length;
                            bSItemPackedBox.FinalWidth = item.ShoppingCartItem.Product.Width;
                            bSItemPackedBox.IsAsShip = true;
                            bSItemPackedBox.BSPackedOrderId = IdPackedOrder;

                            _repository.Insert(bSItemPackedBox);

                            var product = _productServices.GetProductById(item.ShoppingCartItem.ProductId);

                            BSItemPack bSItemPack = new BSItemPack();
                            bSItemPack.Product = product;
                            bSItemPack.ProductId = item.ShoppingCartItem.ProductId;
                            bSItemPack.Qty = item.ShoppingCartItem.Quantity;
                            bSItemPack.BsItemPackBoxId = bSItemPackedBox.Id;
                            bSItemPack.BsItemPackBox = bSItemPackedBox;
                            _itemPack.Insert(bSItemPack);
                        }
					}
					else
					{
                        BSItemPackedBox bSItemPackedBox = new BSItemPackedBox();
                        bSItemPackedBox.PercentBoxVolumePacked = 100;
                        bSItemPackedBox.PackTime = 0;
                        bSItemPackedBox.ItemsPacked = 1;
                        bSItemPackedBox.WeightTotalBox = item.ShoppingCartItem.Product.Weight;
                        bSItemPackedBox.VolumenTotalBox = item.ShoppingCartItem.Product.Height * item.ShoppingCartItem.Product.Width * item.ShoppingCartItem.Product.Length;
                        bSItemPackedBox.GetShippingOptionResponse = JsonConvert.SerializeObject(GetShippingOptions.ShippingOptions);
                        bSItemPackedBox.ContainerPackingResult = "own";
                        bSItemPackedBox.CustomerId = customer.Id;
                        bSItemPackedBox.Customer = customer;
                        bSItemPackedBox.ContainerId = 0;
                        bSItemPackedBox.Active = false;
                        bSItemPackedBox.ContainerName = "own";
                        bSItemPackedBox.FinalHeight = item.ShoppingCartItem.Product.Height;
                        bSItemPackedBox.FinalLength = item.ShoppingCartItem.Product.Length;
                        bSItemPackedBox.FinalWidth = item.ShoppingCartItem.Product.Width;
                        bSItemPackedBox.IsAsShip = true;
                        bSItemPackedBox.BSPackedOrderId = IdPackedOrder;

                        _repository.Insert(bSItemPackedBox);

                        var product = _productServices.GetProductById(item.ShoppingCartItem.ProductId);

                        BSItemPack bSItemPack = new BSItemPack();
                        bSItemPack.Product = product;
                        bSItemPack.ProductId = item.ShoppingCartItem.ProductId;
                        bSItemPack.Qty = item.ShoppingCartItem.Quantity*product.IncrementQuantity;
                        bSItemPack.BsItemPackBoxId = bSItemPackedBox.Id;
                        bSItemPack.BsItemPackBox = bSItemPackedBox;
                        _itemPack.Insert(bSItemPack);
                    }
                }
            }
            else
            {
                var LastItemPackedBox = _repository.Table.Where(x => x.CustomerId == customer.Id && !x.Active && !x.IsAsShip).ToList();

                if (LastItemPackedBox.Count > 0)
                    _repository.Delete(LastItemPackedBox);

                foreach (var item in GetShippingOptions.ResultFinal)
                {
                    foreach (var x in item.AlgorithmPackingResults)
                    {
                        try
                        {
                            BSItemPackedBox bSItemPackedBox = new BSItemPackedBox();
                            bSItemPackedBox.PercentBoxVolumePacked = x.PercentContainerVolumePacked;
                            bSItemPackedBox.PackTime = x.PackTimeInMilliseconds;
                            bSItemPackedBox.ItemsPacked = x.PackedItems.Count;
                            bSItemPackedBox.WeightTotalBox = x.PercentItemWeightPacked;
                            bSItemPackedBox.VolumenTotalBox = x.TotalVolumePacked;
                            bSItemPackedBox.GetShippingOptionResponse = JsonConvert.SerializeObject(GetShippingOptions.ShippingOptions);
                            bSItemPackedBox.ContainerPackingResult = JsonConvert.SerializeObject(x);
                            bSItemPackedBox.CustomerId = customer.Id;
                            bSItemPackedBox.Customer = customer;
                            bSItemPackedBox.ContainerId = x.Container.ID;
                            bSItemPackedBox.Active = false;
                            bSItemPackedBox.ContainerName = x.ContainerName;
                            bSItemPackedBox.FinalHeight = x.Container.Height;
                            bSItemPackedBox.FinalLength = x.Container.Length;
                            bSItemPackedBox.FinalWidth = x.Container.Width;
                            bSItemPackedBox.IsAsShip = false;
                            bSItemPackedBox.BSPackedOrderId = IdPackedOrder;

                            _repository.Insert(bSItemPackedBox);

                            foreach (var y in x.PackedItems)
                            {
                                var product = _productServices.GetProductById(y.ID);

                                BSItemPack bSItemPack = new BSItemPack();
                                bSItemPack.Product = product;
                                bSItemPack.ProductId = y.ID;
                                bSItemPack.Qty = y.Quantity* product.IncrementQuantity;
                                bSItemPack.BsItemPackBoxId = bSItemPackedBox.Id;
                                bSItemPack.BsItemPackBox = bSItemPackedBox;
                                _itemPack.Insert(bSItemPack);
                            }

                        }
                        catch (Exception exc)
                        {
                            var AlgorithmPackingResults = JsonConvert.SerializeObject(x);
                            _logger.Warning(exc.Message + " - NNBoxGeneratorServices- InsertBoxGenerator() -" + AlgorithmPackingResults, exc, customer);
                        }
                    }
                }
            }
        }

        public void DeleteBoxGeneratorByUser(Customer customer, IList<ShoppingCartItem> cart)
        {
            var LastItemPackedBox = _repository.Table.Where(x => x.CustomerId == customer.Id && !x.Active).ToList();

            if (LastItemPackedBox.Count > 0)
                _repository.Delete(LastItemPackedBox);
            
            //foreach (var item in LastItemPackedBox)
            //{
            //    var itemsPacked = _itemPack.Table.Where(r => r.BsItemPackBoxId == item.Id);

            //    var notContains = itemsPacked.Where(t =>t.Product != cart.Select(c=>c.Product)).ToList();

            //    if (notContains.Count() > 0)
            //    {
            //        _repository.Delete(item);
            //    }
            //}
        }

        public void SaveBoxGenerator(Order Order)
        {
            var LastPackedOrder = _packedOrder.Table.Where(x => x.CustomerId == Order.Customer.Id && !x.Active).ToList();

            foreach (var item in LastPackedOrder)
            {
                item.Active = true;
                item.OrderId = Order.Id;

                _packedOrder.Update(item);

                var LastItemPackedBox = _repository.Table.Where(x => x.BSPackedOrderId == item.Id).ToList();

                if (LastItemPackedBox.Count > 0)
                {
                    foreach (var x in LastItemPackedBox)
                    {
                        x.Active = true;
                        x.OrderId = Order.Id;
                        _repository.Update(x);
                    }
                }
            }
        }
        public List<AlgorithmPackingResult> GetBoxByOrder(int OrderId, int CustomerId)
        {
            var LastItemPackedBox = _repository.Table.Where(x => x.CustomerId == CustomerId && x.Active && x.OrderId== OrderId).ToList();
            List<AlgorithmPackingResult> ListItems = new List<AlgorithmPackingResult>();

            if (LastItemPackedBox.Count > 0)
            {
                foreach (var item in LastItemPackedBox)
                {
                    if (item.IsAsShip)
                    {
                        var box = _bsBox.GetById(item.ContainerId);
                        var ContainerPackingResult = new AlgorithmPackingResult();

                        if (item.ContainerPackingResult != "own")
                        {
                            ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(item.ContainerPackingResult);
                        }
                        var ContainerShippingOptions = JsonConvert.DeserializeObject<List<ShippingOption>>(item.GetShippingOptionResponse);

                        if (box == null)
                        {
                            var container = new Container(

                                item.FinalLength,
                                item.FinalWidth,
                                item.FinalHeight,
                                item.WeightTotalBox,
                                0,
                                "own",0
                            );
                            ContainerPackingResult.Container = container;
                            ContainerPackingResult.Id = item.Id;
                            ContainerPackingResult.Active = item.Active;
                            ContainerPackingResult.TotalVolumePacked = item.VolumenTotalBox;
                            ContainerPackingResult.PercentItemWeightPacked = item.WeightTotalBox;
                            ContainerPackingResult.IsAsShip = item.IsAsShip;

                        }
                        ListItems.Add(ContainerPackingResult);
                        
                    }
                    else
                    {
                        var box = _bsBox.GetById(item.ContainerId);
                        var ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(item.ContainerPackingResult);

                        if (box != null)
                        {
                            var container = new Container(
                                box.Length,
                                box.Width,
                                box.Height,
                                box.WeigthAvailable,
                                box.Id,
                                box.Name,box.WeigthBox
                            );
                            ContainerPackingResult.Container = container;
                            ContainerPackingResult.ContainerName = box.Name;
                        }


                        ListItems.Add(ContainerPackingResult);
                    }
                    
                }
            }

            return ListItems;

        }

        public BSBox GetBoxById(int id) 
        {
            BSBox box = _bsBox.GetById(id);

            return box;
        }


        public List<BSPackedOrder> GetBSPackedOrderList(int id)
        {
            var LastItemPackedBox = _packedOrder.Table.Where(r => r.Id == id).ToList();

            return LastItemPackedBox;
        }


        public List<BSItemPackedBox> GetBSItemPackedBoxList(int id)
        {
            var LastItemPackedBox = _repository.Table.Where(r => r.OrderId == id).ToList();

            return LastItemPackedBox;
        }

        public BSItemPackedBox GetItemPackedBoxId(int id)
        {
            var LastItemPackedBox = _repository.Table.Where(r => r.Id == id).FirstOrDefault();

            return LastItemPackedBox;
        }


        public AlgorithmPackingResult PackingById(int id)
        {
            var LastItemPackedBox = _repository.Table.Where(r => r.Id == id).FirstOrDefault();

            AlgorithmPackingResult ListItems = new AlgorithmPackingResult();

            if (LastItemPackedBox != null)
            {

                if (LastItemPackedBox.IsAsShip)
                {
                    var box = _repository.GetById(LastItemPackedBox.ContainerId);
                    AlgorithmPackingResult ContainerPackingResult = new AlgorithmPackingResult();

                    
                    var ContainerShippingOptions = JsonConvert.DeserializeObject<List<ShippingOption>>(LastItemPackedBox.GetShippingOptionResponse);

                    if (box == null)
                    {
                        if (LastItemPackedBox.ContainerPackingResult != "own")
                        {
                            ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(LastItemPackedBox.ContainerPackingResult);
                        }
                        var container = new Container(

                            LastItemPackedBox.FinalLength,
                            LastItemPackedBox.FinalWidth,
                            LastItemPackedBox.FinalHeight,
                            LastItemPackedBox.WeightTotalBox,
                            0,
                            "own",0
                        );
                        ContainerPackingResult.Container = container;
                        ContainerPackingResult.Id = LastItemPackedBox.Id;
                        ContainerPackingResult.Active = LastItemPackedBox.Active;
                        ContainerPackingResult.TotalVolumePacked = LastItemPackedBox.VolumenTotalBox;
                        ContainerPackingResult.PercentItemWeightPacked = LastItemPackedBox.WeightTotalBox;
                        ContainerPackingResult.IsAsShip = LastItemPackedBox.IsAsShip;
                        ContainerPackingResult.PercentBoxVolumePacked = LastItemPackedBox.PercentBoxVolumePacked.ToString();

                        ListItems = ContainerPackingResult;


                    }

                    ListItems.ShippingOption = ContainerShippingOptions;
                }
                else
                {
                    var box = _bsBox.GetById(LastItemPackedBox.ContainerId);
                    var ContainerShippingOptions = JsonConvert.DeserializeObject<List<ShippingOption>>(LastItemPackedBox.GetShippingOptionResponse);

                    if (box != null)
                    {
                        var ContainerPackingResult = JsonConvert.DeserializeObject<AlgorithmPackingResult>(LastItemPackedBox.ContainerPackingResult);

                        var container = new Container(

                            box.Height,
                            box.Width,
                            box.Length,
                            box.WeigthAvailable,
                            box.Id,
                            box.Name, box.WeigthBox
                        );
                        ContainerPackingResult.Container = container;
                        ContainerPackingResult.Id = LastItemPackedBox.Id;
                        ContainerPackingResult.Active = LastItemPackedBox.Active;
                        ListItems = ContainerPackingResult;

                    }

                    ListItems.ShippingOption = ContainerShippingOptions;
                }

            }

            return ListItems;
        }

        public List<BSItemPack> GetBSItemPackList(int id)
        {
            var LastItemPackedBox = _itemPack.Table.Where(r => r.BsItemPackBoxId == id).ToList();

            return LastItemPackedBox;
        }

    }
}
