using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.NN;
using Nop.Core.Domain.Security;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.NN;
using Nop.Services.Security;
using Nop.Services.Seo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public partial class ProductService : IProductService
    {
        #region Fields

        private readonly IConnectionServices _connectionService;
        private readonly IPriceLevelService _priceLevelService;
        private readonly Nop.Services.Catalog.IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly ISettingService _settingService;
        private readonly INotificationService _notificationService;
        private readonly IRepository<Product>  _productRepository;
        private readonly IItemCollectionServices _itemCollection;
        private readonly ILogger _logger;
        private readonly IAclService _aclService;
        private readonly IRepository<PriceByQtyProduct> _priceByQtyProductRepository;
        private IPendingDataToSyncService _pendingDataToSyncService;
        #endregion

        #region Vbles

        private int CategoryId = 0;
        Hashtable hashProducts = new Hashtable();
        Hashtable itemCollectionhashProduct = new Hashtable();
        private string activeallProducts;
        private DateTime LastExecutionDateGeneral;
        #endregion

        #region Ctor

        public ProductService(IConnectionServices connectionService, Nop.Services.Catalog.IProductService productService, Nop.Services.Catalog.ICategoryService categoryService,
            IUrlRecordService urlRecordService, ISettingService settingService, INotificationService notificationService, IRepository<Product> productRepository,
            IItemCollectionServices itemCollection, ILogger logger, IAclService aclService, IRepository<PriceByQtyProduct> priceByQtyProductRepository,
            IPriceLevelService priceLevelService, IPendingDataToSyncService pendingDataToSyncService)
        {
            _priceLevelService = priceLevelService;
            _connectionService = connectionService;
            _productService = productService;
            _categoryService = categoryService;
            _urlRecordService = urlRecordService;
            _settingService = settingService;
            _notificationService = notificationService;
            _productRepository = productRepository;
            _itemCollection = itemCollection;
            _logger = logger;
            _aclService = aclService;
            _priceByQtyProductRepository = priceByQtyProductRepository;
            _pendingDataToSyncService = pendingDataToSyncService;
        }

        #endregion

        #region Methods
        public void ImportProducts(string LastExecutionDate, string idproduct = null, string type = null)
        {
            activeallProducts = _settingService.GetSetting("netsuiteimportermodel.activeallProducts").Value;

            LastExecutionDateGeneral = DateTime.Now;

            if (type == "All")
            {
                ImportProducts(null);
            }
            else if (type == "LastUpdate")
            {
                ImportProducts(LastExecutionDate);
            }
            else if (type == "SpecificCustomerId")
            {
                //  ImportSpecifictOrder(idproduct);
            }
            else if (string.IsNullOrEmpty(type))
            {
                ImportProducts(LastExecutionDate);
            }
        }

        public void ImportProducts(string LastExecutionDate, string idproduct = null, string type = null, List<PendingDataToSync> listProducts = null)
        {
            activeallProducts = _settingService.GetSetting("netsuiteimportermodel.activeallProducts").Value;

            LastExecutionDateGeneral = DateTime.Now;

            if (type == "All")
            {
                ImportProducts(null);
            }
            else if (type == "LastUpdate")
            {
                ImportProducts(LastExecutionDate);
            }
            else if (type == "SpecificCustomerId")
            {
                //  ImportSpecifictOrder(idproduct);
            }
            else if (type == "GetDataFromNetsuite")
            {

                //var getItemUpdateCustomer = _pendingDataToSyncService.GetActivePendingDataToSync().Where(r => r.Type == 2);
                foreach (var item in listProducts)
                {
                    try
                    {
                        _logger.Warning("Start update product: " + item.IdItem);

                        UpdateProductId(LastExecutionDate, item.IdItem);//(item.IdItem.ToString());

                        item.SuccessDate = DateTime.Now;
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = true;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Finished update product: " + item.IdItem);
                    }
                    catch (Exception ex)
                    {
                        item.UpdateDate = DateTime.Now;
                        item.Synchronized = false;
                        item.FailedDate = DateTime.Now;
                        _pendingDataToSyncService.UpdatetPendingDataToSync(item);

                        _logger.Warning("Error update product: " + item.IdItem, ex);
                    }

                }
            }

            else if (string.IsNullOrEmpty(type))
            {
                ImportProducts(LastExecutionDate);
            }
        }

        public void ImportProducts(string LastExecutionDate)
        {
            try
            {
                char delimitador = '/';
                string dateLimit = "";
                int TotalResults = 1;

                //Get All Assembly Items
                GetAssemblyItems(dateLimit, LastExecutionDate);

                //Get All package Items
                GetKitItems(dateLimit, LastExecutionDate);
                var ValidUpdate = false;
                for (int i = 0; i < TotalResults; i += 1000)
                {
                    dateLimit = "?limit=1000&offset=" + i;
                   // Get Customer List from Netsuite
                   ValidUpdate=  GetItemInventory(dateLimit, LastExecutionDate);
                }                

               if(ValidUpdate)
                    RemoveProducts();
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportProductError:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private void RemoveProducts()
        {
            var Products = _productService.GetAllProducts();
            foreach (var item in Products)
            {
                try
                {
                    if (item.IdInventoryItem != null)
                    {
                        if (activeallProducts == "1")
                        {
                            if (!hashProducts.ContainsKey(item.IdInventoryItem))
                            {
                                item.Published = false;
                                item.Deleted = false;
                                _productRepository.Update(item);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    INotificationService _notService = NopEngine._serviceProvider.GetService<INotificationService>();
                    ILogger _log = NopEngine._serviceProvider.GetService<ILogger>();

                    _notService.ErrorNotification(ex.Message);
                    _log.Warning("ImportProductError:: RemoveProducts:: IdProduct" + item.Id + " NameProduct-" + item.Name +  " - Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
                }
            }
        }

        private void GetAssemblyItems(string dateLimit, string LastExecutionDate)
        {
            var kitItem = GetAllAssemblyItem()?.Items;
            

           //var AssemblyItems = _settingService.GetSetting("AssemblyItems.Id").Value;
           // var charArray = AssemblyItems.Split(",");

            foreach (var item in kitItem)
            {
                var id = item.Id.ToString();
                
                var existProduct = _productService.GetProductsByNetsuiteItem(id);
                var productDto = GetAssemblyItemId(Convert.ToInt32(id));
                if (productDto != null)
                {
                    if (productDto.IsOnline)
                    {
                        bool activeAll = true;
                        if (activeallProducts != "1")
                        {
                            var dateLast = Convert.ToDateTime(LastExecutionDate).AddDays(-4).Date;
                            var dateLast2 = Convert.ToDateTime(productDto.LastModifiedDate).Date;

                            activeAll = dateLast2.Date > dateLast.Date;
                        }

                        if (activeAll)
                        {
                            if (!hashProducts.ContainsKey(productDto.Id))
                                hashProducts.Add(productDto.Id, productDto);

                            if (existProduct == null)
                                existProduct = InsertProduct(productDto, "assemblyitem");
                            else
                            {
                                if (productDto != null)
                                {
                                    var getItemCollectionbyProductId = _itemCollection.GetItemCollectionProductById(existProduct.Id);
                                    if (!itemCollectionhashProduct.ContainsKey(existProduct.IdInventoryItem))
                                    {
                                        if (getItemCollectionbyProductId.Count > 0)
                                            itemCollectionhashProduct.Add(existProduct.Id, getItemCollectionbyProductId);
                                    }

                                    UpdateProduct(productDto, existProduct, "assemblyitem");

                                    
                                }
                            }
                            CreateCategory(productDto, existProduct, "assemblyitem");
                        }
                    }
                }
            }
        }

        private void GetKitItems(string dateLimit, string LastExecutionDate)
        {
            var KitItems = GetAllkitItem()?.Items;

            //var KitItems = _settingService.GetSetting("KitItems.Id").Value;
           // var charArray = KitItems.Split(",");
            Nop.Services.Catalog.IProductService _productServices = NopEngine._serviceProvider.GetService<Nop.Services.Catalog.IProductService>();

            foreach (var item in KitItems)
            {
                var id = item.Id.ToString();
               
                var existProduct = _productServices.GetProductsByNetsuiteItem(id);
                var productDto = GetPackageItemId(Convert.ToInt32(id));

                if (productDto != null)
                {
                    bool activeAll = true;
                    if (activeallProducts != "1")
                    {
                        var dateLast = Convert.ToDateTime(LastExecutionDate).AddDays(-4).Date;
                        var dateLast2 = Convert.ToDateTime(productDto.LastModifiedDate).Date;

                        activeAll = dateLast2.Date > dateLast.Date;
                    }

                    if (activeAll)
                    {
                        if (productDto.IsOnline)
                        {
                            if (!hashProducts.ContainsKey(productDto.Id))
                                hashProducts.Add(productDto.Id, productDto);
                            if (existProduct == null)
                            {
                                existProduct = InsertProduct(productDto, "kitItem");
                            }
                            else
                            {
                                if (productDto != null)
                                {
                                    var getItemCollectionbyProductId = _itemCollection.GetItemCollectionProductById(existProduct.Id);
                                    if (!itemCollectionhashProduct.ContainsKey(existProduct.IdInventoryItem))
                                    {
                                        if (getItemCollectionbyProductId.Count > 0)
                                            itemCollectionhashProduct.Add(existProduct.Id, getItemCollectionbyProductId);
                                    }

                                    UpdateProduct(productDto, existProduct, "kitItem");
                                    
                                   
                                }
                                    
                            }
                            CreateCategory(productDto, existProduct, "kitItem");
                        }
                    }

                }
            }
        }

        private bool GetItemInventory(string dateLimit, string LastExecutionDate)
        {
            int indexTask = 1;
            var ListInventoryItem = GetItemInvetoryList(dateLimit, LastExecutionDate);
            try
            {
                if (ListInventoryItem.Count > 0)
                {
                    foreach (var item in ListInventoryItem.Items)
					{
						UpdateProductId(LastExecutionDate, item.Id);

						indexTask++;
					}

					return true;
                }
            }
            catch (Exception ex)
            {
                INotificationService _notService = NopEngine._serviceProvider.GetService<INotificationService>();
                ILogger _log = NopEngine._serviceProvider.GetService<ILogger>();

                _notService.ErrorNotification(ex.Message);
                _log.Warning("ImportProductError:: GetItemInventory ::" + indexTask + " Detail:: " + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);

            }
            return false;
        }

		private void UpdateProductId(string LastExecutionDate, int itemId)
		{
			var existProduct = _productService.GetProductsByNetsuiteItem(itemId.ToString());

			var productDto = GetItemInvetoryId(itemId);
			if (productDto != null)
			{
				bool activeAll = true;
				if (activeallProducts != "1")
				{
					var dateLast = Convert.ToDateTime(LastExecutionDate).AddDays(-4).Date;
					var dateLast2 = Convert.ToDateTime(productDto.LastModifiedDate).Date;

					activeAll = dateLast2.Date > dateLast.Date;
				}

				if (activeAll)
				{
					if (productDto.IsOnline)
					{
						if (!hashProducts.ContainsKey(productDto.Id))
							hashProducts.Add(productDto.Id, productDto);

						if (existProduct == null)
						{
							existProduct = InsertProduct(productDto, "inventoryitem");
						}
						else
						{
							if (productDto != null)
							{
								var getItemCollectionbyProductId = _itemCollection.GetItemCollectionProductById(existProduct.Id);
								if (!itemCollectionhashProduct.ContainsKey(existProduct.IdInventoryItem))
								{
									if (getItemCollectionbyProductId.Count > 0)
										itemCollectionhashProduct.Add(existProduct.Id, getItemCollectionbyProductId);
								}
								UpdateProduct(productDto, existProduct, "inventoryitem");
							}
						}

						CreateCategory(productDto, existProduct, "inventoryitem");
					}
					else
					{
						if (existProduct != null)
						{
                            if (existProduct.Published != productDto.IsOnline)
                            {
                                existProduct.Published = productDto.IsOnline;

                                _productService.UpdateProduct(existProduct);
                            }
                        }
                        

                    }
				}
			}
		}

		private void UpdateProduct(ProductDto productDto, Product product, string typeItem)
        {
            try
            {
                if (product.Id == 3243)
                { 
                }
                List<PriceByQtyProduct> listPrices = new List<PriceByQtyProduct>();
                var PriceBase = GetItemInvetoryPrice(productDto.Id, listPrices, typeItem);

                var ShortDescription = Regex.Replace(productDto.storeDescription, "<.*?>", String.Empty);
                ShortDescription = Regex.Replace(ShortDescription, "&nbsp;", String.Empty);

                var FullDescription = Regex.Replace(productDto.storeDetailedDescription, "<.*?>", String.Empty);
                FullDescription = Regex.Replace(FullDescription, "&nbsp;", String.Empty);


                if (string.IsNullOrEmpty(productDto.storeDescription))
                    ShortDescription = productDto.SalesDescription;

                if (string.IsNullOrEmpty(FullDescription))
                    FullDescription = productDto.PurchaseDescription;

                if(productDto.Custitem_price_list_display_name!=null)
                    product.Name = productDto.Custitem_price_list_display_name;
                else
                    product.Name = "WithoutName";

               
                product.ShortDescription = ShortDescription;
                product.FullDescription = FullDescription;
                product.ShipSeparately = productDto.ShipIndividually;
                product.BasepriceAmount = PriceBase.Price;
                product.Sku = productDto.Id;
                product.Price = PriceBase.Price;
                product.Published = productDto.IsOnline;
                product.Deleted = productDto.IsInactive;
                product.UpdatedOnUtc = productDto.LastModifiedDate;
                product.Weight = productDto.Weight;
                product.Length = productDto.ShipLength;
                product.Width = productDto.ShipWidth;
                product.Height = productDto.ShipHeight;
                product.LoadCapacity = productDto.LoadCapacity;
                product.ShipLength = productDto.ProductLength;
                product.ShipWidth = productDto.ProductWidth;
                product.ShipHeight = productDto.ProductHeight;
                product.SpecificationClass = productDto.SpecificationClass?.RefName;
                product.SpecificationClassId = productDto.SpecificationClass?.Id;
                product.UnShippable = productDto.UnShippable;

                product.OutStock = productDto.custitem_web_sold_out;
                //validate search engine name
                _productService.UpdateProduct(product);

               
                string productName = product.Name.ToString();
                productName = productName.Replace(" ", "-");
                productName = productName.Replace("---", "-");
                productName = productName.Replace("/", "-");
                productName = productName.Replace("\"", "");
                productName = productName.Replace("(", "-");
                productName = productName.Replace(")", "-");
                productName = productName.Replace("'", "");
                
                // var Slug = _urlService.GetActiveSlug(product.Id, productName, 0);
                var Slug = _urlRecordService.GetActiveSlug(product.Id, productName, 0);
                if (string.IsNullOrEmpty(Slug))
                {
                    //IUrlRecordService _urlService2 = NopEngine._serviceProvider.GetService<IUrlRecordService>();

                    var SlugInfo = _urlRecordService.GetBySlug(productName);

                    //IUrlRecordService _urlService3 = NopEngine._serviceProvider.GetService<IUrlRecordService>();

                    if (SlugInfo==null)
                        _urlRecordService.SaveSlug(product, productName, 0);
                    else
                    {
                        if(product.Id!= SlugInfo.EntityId)
                        {
                            productName = productName + "_" + product.Id;
                            _urlRecordService.SaveSlug(product, productName, 0);
                        }
                    }
                }

                ItemCollection(productDto, product);

               var items = GetcBrandedVariants(productDto.Id);
                var listItemsCollection = "";

				if (items != null)
				{
                    foreach (var item in items.items)
                    {
                        listItemsCollection += item.Id + ",";

                    }
                }
				
                product.Custitem_branded_variants = listItemsCollection;

                //Se almacenan los precios x cantidad
                //IPriceLevelService _priceLevel = NopEngine._serviceProvider.GetService<IPriceLevelService>();

                //   IRepository<PriceByQtyProduct> _priceByQtyProductRepository = NopEngine._serviceProvider.GetService<IRepository<PriceByQtyProduct>>();
                if (product.Id != 0)
                {
                    var prices = _priceByQtyProductRepository.GetListByWhere(s => s.ProductId == product.Id);

                    if (prices != null && prices.Count > 0)
                        _priceByQtyProductRepository.Delete(prices);
                }

              //  _priceLevel.DeletePriceByQtyProductByProductId(product.Id);

                if (listPrices != null && listPrices.Count > 0)
                    foreach (var priceByQtyProduct in listPrices)
                    {
                        //IRepository<PriceByQtyProduct> _priceLevelRepositor = NopEngine._serviceProvider.GetService<IRepository<PriceByQtyProduct>>();

                        priceByQtyProduct.ProductId = product.Id;
                        _priceLevelService.InsertPriceByQtyProduct(priceByQtyProduct);
                    }
            }
            catch (Exception ex)
            {
                //INotificationService _notService = NopEngine._serviceProvider.GetService<INotificationService>();
                //ILogger _log = NopEngine._serviceProvider.GetService<ILogger>();

                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportProductError:: Update Product:: IdNetsuite -"+ product.IdInventoryItem +"IdNP-"+ product.Id +" Detail::" + ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }


        }

        private void CreateCategory(ProductDto productDto, Product product, string itemType)
        {
            CategoryId = 0;
            Hashtable categoryProductList = new Hashtable();

            try
            {
                if (productDto != null)
                {
                    if (productDto.CategorySite != null)
                    {
                        if (productDto.CategorySite.Links != null)
                        {
                            var CategoryList = GetCategoryDetail(productDto.Id, itemType);

                            var categoryProduct = _categoryService.GetProductCategoriesByProductId(product.Id);

                            if (CategoryList.Count > 0)
                            {
                                categoryProductList = new Hashtable();
                                foreach (var item in categoryProduct)
                                {
                                    categoryProductList.Add(item.CategoryId, item);

                                    _categoryService.DeleteProductCategory(item);
                                }
                            }
                            

                            foreach (var categoryDetail in CategoryList)
                            {
                                CategoryId = 0;
                                var nameCategory = categoryDetail.refName;
                                if (nameCategory != null)
                                {
                                    string[] cat = nameCategory.Split(":");
                                   
                                    int parent0 = 0;
                                    int parent1 = 0;
                                    int parent2 = 0;
                                    int parent3 = 0;
                                    for (int i = 0; i < cat.Length; i++)
                                    {
                                        //validate category 
                                        var category = _categoryService.GetProductsByCategorynameNetsuite(cat[i].ToLower().Trim(), Convert.ToInt32(categoryDetail.Id), CategoryId);

                                        if (category != null)
                                        {

                                            CategoryId = category.Id;


                                            if (i == 0)
                                                parent0 = category.Id;

                                            if (i == 1)
                                                parent1 = category.Id;

                                            if (i == 2)
                                                parent2 = category.Id;

                                            if (i == 3)
                                                parent3 = category.Id;
                                        }
                                        else
                                        {
                                            var categoryValidName = _categoryService.GetProductsByCategorynameNetsuiteValid(cat[i].ToLower().Trim(), Convert.ToInt32(categoryDetail.Id));

                                            if (categoryValidName == null)
                                            {

                                                Category categoryInsert = new Category();
                                                categoryInsert.Name = cat[i].Trim();
                                                categoryInsert.NetSuiteId = Convert.ToInt32(categoryDetail.Id);
                                                categoryInsert.Published = true;
                                                categoryInsert.IncludeInTopMenu = true;
                                                categoryInsert.AllowCustomersToSelectPageSize = true;
                                                categoryInsert.PageSizeOptions = "6, 3, 9";

                                                if (i == 0)
                                                {
                                                    categoryInsert.ShowOnHomepage = true;
                                                    categoryInsert.ParentCategoryId = 0;
                                                }

                                                if (i == 1)
                                                    categoryInsert.ParentCategoryId = parent0;

                                                if (i == 2)
                                                    categoryInsert.ParentCategoryId = parent1;

                                                if (i == 3)
                                                    categoryInsert.ParentCategoryId = parent2;

                                                if (i == 4)
                                                    categoryInsert.ParentCategoryId = parent3;

                                                _categoryService.InsertCategory(categoryInsert);

                                                string categoryName = categoryInsert.Name.ToString();
                                                categoryName = categoryName.Replace(" ", "-");

                                                var Slug = _urlRecordService.GetActiveSlug(categoryInsert.Id, categoryName, 0);
                                                if (string.IsNullOrEmpty(Slug))
                                                {
                                                    var SlugInfo = _urlRecordService.GetBySlug(categoryName);
                                                    if (SlugInfo == null)
                                                        _urlRecordService.SaveSlug(categoryInsert, categoryName, 0);
                                                    else
                                                    {
                                                        categoryName = categoryName + "_" + categoryInsert.Id;
                                                        _urlRecordService.SaveSlug(categoryInsert, categoryName, 0);
                                                    }
                                                }

                                                CategoryId = categoryInsert.Id;


                                                if (i == 0)
                                                    parent0 = categoryInsert.Id;

                                                if (i == 1)
                                                    parent1 = categoryInsert.Id;

                                                if (i == 2)
                                                    parent2 = categoryInsert.Id;

                                                if (i == 3)
                                                    parent3 = categoryInsert.Id;

                                            }
                                            else
                                            {
                                                categoryValidName.NetSuiteId = Convert.ToInt32(categoryDetail.Id);
                                                _categoryService.UpdateCategory(categoryValidName);

                                                CategoryId = categoryValidName.Id;

                                                if (i == 0)
                                                    parent0 = categoryValidName.Id;

                                                if (i == 1)
                                                    parent1 = categoryValidName.Id;

                                                if (i == 2)
                                                    parent2 = categoryValidName.Id;

                                                if (i == 3)
                                                    parent3 = categoryValidName.Id;
                                            }
                                        }

                                        if (i == cat.Length - 1)
                                        {
                                            var categoryInsert = CategoryId;

                                            if (parent0 != 0)
                                                categoryInsert = parent0;

                                            if (parent1 != 0)
                                                categoryInsert = parent1;

                                            if (parent2 != 0)
                                                categoryInsert = parent2;

                                            if (parent3 != 0)
                                                categoryInsert = parent3;

                                            ProductCategoryMapping(productDto, product, categoryInsert, _categoryService, categoryProductList);
                                        }

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportProductError:: Create Category:: IDNP- "+ product.Id +" IdNetsuite- "+product.IdInventoryItem +" Detail:: "+ ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
        }

        private Product InsertProduct(ProductDto product, string inventoryitem)
        {
            List<PriceByQtyProduct> listPrices = new List<PriceByQtyProduct>();

            var PriceBase = GetItemInvetoryPrice(product.Id, listPrices, inventoryitem);

            var ShortDescription = product.storeDescription;

            if (product.storeDescription != null)
            {
                ShortDescription = Regex.Replace(product.storeDescription, "<.*?>", String.Empty);
                ShortDescription = Regex.Replace(ShortDescription, "&nbsp;", String.Empty);
            }

            var FullDescription = product.storeDetailedDescription;

            if (product.storeDetailedDescription != null)
            {
                FullDescription = Regex.Replace(product.storeDetailedDescription, "<.*?>", String.Empty);
                FullDescription = Regex.Replace(FullDescription, "&nbsp;", String.Empty);
            }

            

            if (string.IsNullOrEmpty(ShortDescription))
                ShortDescription = product.SalesDescription;

            if (string.IsNullOrEmpty(FullDescription))
                FullDescription = product.PurchaseDescription;

            var Name = "";
            if (!string.IsNullOrEmpty(product.Custitem_price_list_display_name))
                Name = product.Custitem_price_list_display_name;
            else
                Name = "No Name";
            // product
            var productCopy = new Product
            {
                ProductTypeId = 5,
                ParentGroupedProductId = 0,
                VisibleIndividually = true,
                Name = Name,
                ShortDescription = ShortDescription,
                FullDescription = FullDescription,
                VendorId = 0,
                ProductTemplateId = 1,
                Sku = product.Id,
                ShowOnHomepage = false,
                AllowCustomerReviews = false,
                LimitedToStores = false,
                IsGiftCard = false,
                GiftCardType = 0,
                RequireOtherProducts = false,
                AutomaticallyAddRequiredProducts = false,
                IsDownload = false,
                DownloadId = 0,
                UnlimitedDownloads = false,
                MaxNumberOfDownloads = 0,
                DownloadActivationType = 0,
                HasSampleDownload = false,
                SampleDownloadId = 0,
                HasUserAgreement = false,
                IsRecurring = false,
                RecurringCycleLength = 0,
                RecurringCyclePeriod = 0,
                RecurringTotalCycles = 0,
                IsRental = false,
                RentalPriceLength = 0,
                RentalPricePeriod = 0,
                IsShipEnabled = true,
                IsFreeShipping = false,
                ShipSeparately = product.ShipIndividually,
                AdditionalShippingCharge = 0,
                DeliveryDateId = 0,
                IsTaxExempt = false,
                TaxCategoryId = 0,
                IsTelecommunicationsOrBroadcastingOrElectronicServices =
                   false,
                ManageInventoryMethod = 0,
                ProductAvailabilityRangeId = 0,
                UseMultipleWarehouses = false,
                WarehouseId = 0,
                StockQuantity = 10000,
                DisplayStockAvailability = false,
                DisplayStockQuantity = false,
                MinStockQuantity = 0,
                LowStockActivityId = 1,
                NotifyAdminForQuantityBelow = 0,
                BackorderMode = 0,
                AllowBackInStockSubscriptions = false,
                OrderMinimumQuantity = 1,
                OrderMaximumQuantity = 1000,
                AllowAddingOnlyExistingAttributeCombinations = false,
                NotReturnable = false,
                DisableBuyButton = false,
                DisableWishlistButton = false,
                AvailableForPreOrder = false,
                CallForPrice = false,
                Price = PriceBase.Price,
                OldPrice = 0,
                ProductCost = 0,
                CustomerEntersPrice = false,
                MinimumCustomerEnteredPrice = 0,
                MaximumCustomerEnteredPrice = 0,
                BasepriceEnabled = false,
                BasepriceAmount = PriceBase.Price,
                BasepriceUnitId = 0,
                BasepriceBaseAmount = 0,
                BasepriceBaseUnitId = 0,
                MarkAsNew = false,
                Weight = product.Weight,
                Length = product.ShipLength,
                Width = product.ShipWidth,
                Height = product.ShipHeight,
                LoadCapacity = product.LoadCapacity,
                ShipLength = product.ProductLength,
                ShipWidth = product.ProductWidth,
                ShipHeight = product.ProductHeight,
                DisplayOrder = 0,
                Published = product.IsOnline,
                Deleted = product.IsInactive,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = product.LastModifiedDate,
                IdInventoryItem = product.Id,
                SpecificationClass = product.SpecificationClass?.RefName,
                SpecificationClassId = product.SpecificationClass?.Id,
                UnShippable = product.UnShippable,
                IncrementQuantity=1,
            };

            try
            {
                //validate search engine name
                _productRepository.Insert(productCopy);

                string productName = productCopy.Name.Trim().ToString();
                productName = productName.Replace(" ", "-");
                productName = productName.Replace("---", "-");
                productName = productName.Replace("/", "-");
                productName = productName.Replace("\"", "");
                productName = productName.Replace("(", "-");
                productName = productName.Replace(")", "-");
                productName = productName.Replace("'", "");

                var Slug = _urlRecordService.GetActiveSlug(productCopy.Id, productName, 0);
                if (string.IsNullOrEmpty(Slug))
                {
                    var SlugInfo = _urlRecordService.GetBySlug(productName);
                    if (SlugInfo == null)
                        _urlRecordService.SaveSlug(productCopy, productName, 0);
                    else
                    {
                        productName = productName + "_" + productCopy.Id;
                        _urlRecordService.SaveSlug(productCopy, productName, 0);
                    }
                }

                ItemCollection(product, productCopy);
                //Se almacenan los precios x cantidad
                if (productCopy.Id != 0)
                {
                    var prices = _priceByQtyProductRepository.GetListByWhere(s => s.ProductId == productCopy.Id);
                    if (prices != null && prices.Count > 0)
                        _priceByQtyProductRepository.Delete(prices);
                }
                if (listPrices != null && listPrices.Count > 0)
                    foreach (var priceByQtyProduct in listPrices)
                    {
                        priceByQtyProduct.ProductId = productCopy.Id;
                        _priceByQtyProductRepository.Insert(priceByQtyProduct);
                    }

            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("ImportProductError:: InsertProduct :: " + "IdNetsuite -" + product.Id + " Detail:: "+ ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }
            return productCopy;
        }

        private void ProductCategoryMapping(ProductDto product, Product productCopy, int categoryId, ICategoryService _catService, Hashtable categoryProductList)
        {
            var GetProductCategoriesByProductId = _categoryService.GetProductCategoriesByProductId(productCopy.Id);
            var categoryExist = GetProductCategoriesByProductId.Where(r => r.CategoryId == categoryId).FirstOrDefault();

            if (categoryExist != null)
            {
                //if (categoryId != 0)
                //{
                //    categoryExist.ProductId = productCopy.Id;
                //    categoryExist.CategoryId = categoryId;
                //    _categoryService.UpdateProductCategory(categoryExist);
                //}
                //else
                //    _logger.Information(" product without category " + product.Id);
            }
            else
            {
                if (categoryId != 0)
                {
                    var DisplayOrder = 0;

                    var CategoryExist = categoryProductList.Contains(categoryId);

                    foreach (DictionaryEntry de in categoryProductList)
                    {
                        var CategoryInfo = (ProductCategory)de.Value;
                        if (CategoryInfo != null)
                        {
                            if (CategoryInfo.CategoryId == categoryId && CategoryInfo.ProductId== productCopy.Id)
                            {
                                DisplayOrder = CategoryInfo.DisplayOrder;
                            }
                        }
                    }

                    if (CategoryExist != null)
                    {
                        ProductCategory productCategory = new ProductCategory();
                        productCategory.ProductId = productCopy.Id;
                        productCategory.CategoryId = categoryId;
                        productCategory.DisplayOrder = DisplayOrder;
                        _catService.InsertProductCategory(productCategory);

                    }
                    else {
                        ProductCategory productCategory = new ProductCategory();
                        productCategory.ProductId = productCopy.Id;
                        productCategory.CategoryId = categoryId;
                        _catService.InsertProductCategory(productCategory);
                    }

                }
                else
                {
                  //  ILogger _log = NopEngine._serviceProvider.GetService<ILogger>();
                   // _log.Information(" product without category " + product.Id);
                }
            }
        }

        private void ItemCollection(ProductDto product, Product productCopy)
        {
            try
            {
                if(product.Id== "2234")
                {

                }
                if (product.custitem_pricing_group != null)
                {
                    var collection = _itemCollection.GetItemCollectionById(Convert.ToInt32(product.custitem_pricing_group?.Id));
                    if (collection == null)
                    {
                        var ProductId = Convert.ToInt32(product.custitem_pricing_group.Id);
                        var collectionName = product.custitem_pricing_group.RefName;
                        ItemCollection itemCollection = CreateItemCollectionCustomer(ProductId, collectionName);

                        var collectionAdmin = _itemCollection.GetItemCollectionCompanyByIdCollection(1, itemCollection.Id);
                        if (collectionAdmin.Count == 0)
                            CreateItemCollectionAdmin(itemCollection.Id);

                        ItemCollectionProduc itemCollectionProduc = CreateItemCollectionProdut(productCopy, itemCollection);
                        //update product
                        if (itemCollectionProduc.Id != 0)
                        {
                            var aclRecord = new AclRecord
                            {
                                EntityId = productCopy.Id,
                                EntityName = "Product",
                                CustomerRoleId = 6
                            };
                            _aclService.InsertAclRecord(aclRecord);

                            productCopy.SubjectToAcl = true;
                            _productService.UpdateProduct(productCopy);
                        }

                    }
                    else
                    {
                        if (itemCollectionhashProduct.ContainsKey(productCopy.Id))
                        {
                            var itemCollectionProducs = (List<ItemCollectionProduc>)itemCollectionhashProduct[productCopy.Id];

                            var ExistItemCollection = itemCollectionProducs.Where(r => r.CollectionId == collection.Id && r.ProductId == productCopy.Id);

                            if (ExistItemCollection.Count() > 0)
                            {
                                foreach (var x in ExistItemCollection)
                                {
                                    itemCollectionProducs.Remove(x);
                                }
                            }
                        }
                        
                        if(collection.Name != product.custitem_pricing_group.RefName)
                        {
                            collection.Name = product.custitem_pricing_group.RefName;
                            _itemCollection.UpdateIItemCollection(collection);
                        }

                        var collectionProduct = _itemCollection.GetItemCollectionProductById(Convert.ToInt32(productCopy.Id));
                        if (collectionProduct.Count == 0)
                        {
                            ItemCollectionProduc itemCollectionProduc = CreateItemCollectionProdut(productCopy, collection);
                        }

                        var collectionProductItem = _itemCollection.GetItemCollectionProductByCollectionId(Convert.ToInt32(productCopy.Id), collection.Id);
                        if (collectionProductItem.Count == 0)
                        {
                            ItemCollectionProduc itemCollectionProductCollection = CreateItemCollectionProdut(productCopy, collection);
                        }

                        var collectionAdmin = _itemCollection.GetItemCollectionCompanyByIdCollection(1, collection.Id);
                        if (collectionAdmin.Count == 0)
                            CreateItemCollectionAdmin(collection.Id);


                        var aclRecord = _aclService.GetAclRecordById(productCopy.Id);

                        //update product
                        if (aclRecord == null)
                        {
                            var aclRecordUpdate = new AclRecord
                            {
                                EntityId = productCopy.Id,
                                EntityName = "Product",
                                CustomerRoleId = 6
                            };
                            _aclService.InsertAclRecord(aclRecordUpdate);

                            productCopy.SubjectToAcl = true;
                            _productService.UpdateProduct(productCopy);
                        }
                    }
                }
               
            }
            catch (Exception ex)
            {
                _notificationService.ErrorNotification(ex.Message);
                _logger.Warning("Error ItemCollection NetsuiteId- " + product.Id+" IdNP- "+ productCopy.Id +" Detail "+ ex.Message + " LastExecutionDateGeneral:: " + LastExecutionDateGeneral);
            }

        }

        private ItemCollectionProduc CreateItemCollectionProdut(Product productCopy, ItemCollection itemCollection)
        {
            //create product item collection
            ItemCollectionProduc itemCollectionProduc = new ItemCollectionProduc();
            itemCollectionProduc.CollectionId = itemCollection.Id;
            itemCollectionProduc.ProductId = productCopy.Id;
            _itemCollection.InsertItemCollectionProduct(itemCollectionProduc);
            return itemCollectionProduc;
        }

        private ItemCollection CreateItemCollectionCustomer(int productId, string collectionName)
        {
            ItemCollection itemCollection = new ItemCollection();
            itemCollection.NetsuiteId = productId;
            itemCollection.Name = collectionName;
            _itemCollection.InsertItemCollection(itemCollection);

            return itemCollection;
        }

        private void CreateItemCollectionAdmin(int itemCollectionId)
        {
            ItemCollectionCompany itemCollectionCompany = new ItemCollectionCompany();
            itemCollectionCompany.CustomerId = 1;
            itemCollectionCompany.CollectionId = itemCollectionId;
            itemCollectionCompany.CustomerNetsuiteId = 0;
            _itemCollection.InsertItemCollectionCompany(itemCollectionCompany);
        }

        #endregion

        #region Netsuite Methods

        private List<LinksObject> GetCategoryDetail(string id, string itemtype)
        {
            var category = _connectionService.GetConnection("/record/v1/"+ itemtype + "/" + id + "/custitem26", "GET");

            List<LinksObject> catList = new List<LinksObject>();

            var categoryList = JsonConvert.DeserializeObject<LinkRelObjectCat>(category);
			if (categoryList != null)
			{
                if (categoryList.Items.Count > 0)
                {
                    foreach (var a in categoryList.Items)
                    {
                        LinksObject cat = new LinksObject();
                        cat.Id = a.Id;
                        cat.refName = a.refName;
                        catList.Add(cat);
                    }
                }
            }
            

            return catList;
        }
        private ItemInventoryPrice GetItemInvetoryPrice(string id, List<PriceByQtyProduct> listPrices, string inventoryitem)
        {
            var prices = _connectionService.GetConnection("/record/v1/"+ inventoryitem + "/" + id + "/price", "GET");

            ItemInventoryPrice BasePrice = new ItemInventoryPrice();
            Hashtable hashPriceLevelExecuted = new Hashtable();
            Hashtable hashPricesExecuted = new Hashtable();


            var priceList = JsonConvert.DeserializeObject<ItemInventoryPriceList>(prices);

            if (priceList.Items.Length > 0)
            {
                foreach (var a in priceList.Items)
                {
                    foreach (var j in a.Links)
                    {
                        string link = j.Href;
                        string[] allPrice = link.Split('/');

                        if (allPrice.Length > 0)
                        {
                            string price = allPrice[allPrice.Length - 1];
                            var item = GetPrice(id, price, inventoryitem);

                            //// Precio BASE
                            if (item.PriceLevel.Id == "1" && int.Parse(item.Quantity.Value) == 0)
                                BasePrice.Price = item.Price;



                            var priceLevel = new PriceLevel()
                            {
                                IdNetSuite = item.PriceLevel.Id,
                                Name = item.PriceLevelName
                            };

                            if (!hashPriceLevelExecuted.ContainsKey(item.PriceLevel.Id))
                            {
                                var currentExistsPriceLevel = _priceLevelService.GetExistsPriceLevel(item.PriceLevel.Id);
                                if (currentExistsPriceLevel == null)
                                {
                                    _priceLevelService.InsertPriceLevel(priceLevel);
                                    hashPriceLevelExecuted.Add(item.PriceLevel.Id, priceLevel);
                                }
                                else
                                {
                                    if (currentExistsPriceLevel.Name != item.PriceLevelName)
                                    {
                                        currentExistsPriceLevel.Name = item.PriceLevelName;
                                        _priceLevelService.UpdatePriceLevel(currentExistsPriceLevel);
                                    }

                                    hashPriceLevelExecuted.Add(item.PriceLevel.Id, currentExistsPriceLevel);
                                }
                            }

                            priceLevel = (PriceLevel)hashPriceLevelExecuted[item.PriceLevel.Id];

                            if (!hashPricesExecuted.ContainsKey($"{id}_{item.PriceLevel.Id}_{int.Parse(item.Quantity.Value)}"))
                            {
                                listPrices.Add(new PriceByQtyProduct()
                                {
                                    Price = item.Price,
                                    PriceLevelId = priceLevel.Id,
                                    Quantity = int.Parse(item.Quantity.Value)
                                });

                                hashPricesExecuted.Add($"{id}_{item.PriceLevel.Id}_{int.Parse(item.Quantity.Value)}", true);
                            }
                        }
                    }
                }

            }

            return BasePrice;
        }

        private ItemInventoryPrice GetPrice(string id, string v, string itemType)
        {
            var GetData = _connectionService.GetConnection("/record/v1/"+ itemType + "/" + id + "/price/" + v, "GET");
            var price = JsonConvert.DeserializeObject<ItemInventoryPrice>(GetData);
            return price;

        }

        private EntityLinkRefObjectList GetcBrandedVariants(string id)
        {
            var GetData = _connectionService.GetConnection("/record/v1/inventoryitem/" + id + "/custitem_branded_variants", "GET");
            var BrandedVariants = JsonConvert.DeserializeObject<EntityLinkRefObjectList>(GetData);
            return BrandedVariants;

        }

        private ItemInventoryObject GetItemInvetoryList(string dateLimit, string LastExecutionDate)
        {
            var GetData = _connectionService.GetConnection("/record/v1/inventoryitem?q=isOnline IS true", "GET");
            var ListItem = JsonConvert.DeserializeObject<ItemInventoryObject>(GetData);
            return ListItem;
        }

        private ProductDto GetItemInvetoryId(int id)
        {
            var GetData = _connectionService.GetConnection("/record/v1/inventoryitem/" + id, "GET");
            var productDto = JsonConvert.DeserializeObject<ProductDto>(GetData);
            return productDto;

        }

        private ProductDto GetAssemblyItemId(int id)
        {
            var GetData = _connectionService.GetConnection("/record/v1/assemblyitem/" + id, "GET");
            var productDto = JsonConvert.DeserializeObject<ProductDto>(GetData);
            return productDto;

        }

        private ProductDto  GetPackageItemId(int id)
        {
            var GetData = _connectionService.GetConnection("/record/v1/kitItem/" + id, "GET");
            var productDto = JsonConvert.DeserializeObject<ProductDto>(GetData);
            return productDto;

        }

        private TransactionListDto GetAllkitItem()
        {
            var GetData = _connectionService.GetConnection("/record/v1/kitItem/" , "GET");
            var productDto = JsonConvert.DeserializeObject<TransactionListDto>(GetData);
            return productDto;

        }

        private TransactionListDto GetAllAssemblyItem()
        {
            var GetData = _connectionService.GetConnection("/record/v1/assemblyitem/", "GET");
            var productDto = JsonConvert.DeserializeObject<TransactionListDto>(GetData);
            return productDto;

        }

        #endregion

        #region Public Methods
        public Product CreateProductbyNetsuite(OrderItemDto item)
        {
            var productDto = new ProductDto();
            Product product = new Product();

            if (!string.IsNullOrEmpty(item.item.id))
                productDto = GetItemInvetoryId(Convert.ToInt32(item.item.id));

            if (productDto == null)
            {
                var name = item.description;
                if (item.description == null)
                    name = item.item?.refName;
                // product
                var productCopy = new Product
                {
                    ProductTypeId = 5,
                    ParentGroupedProductId = 0,
                    VisibleIndividually = true,
                    Name = name,
                    ShortDescription = item.description,
                    FullDescription = item.description,
                    VendorId = 0,
                    ProductTemplateId = 1,
                    //AdminComment = product.AdminComment,
                    ShowOnHomepage = false,
                    //MetaKeywords = product.MetaKeywords,
                    //MetaDescription = product.MetaDescription,
                    //MetaTitle = product.MetaTitle,
                    AllowCustomerReviews = false,
                    LimitedToStores = false,
                    //Sku = newSku,
                    //ManufacturerPartNumber = product.ManufacturerPartNumber,
                    //Gtin = product.Gtin,
                    IsGiftCard = false,
                    GiftCardType = 0,
                    //OverriddenGiftCardAmount = product.OverriddenGiftCardAmount,
                    RequireOtherProducts = false,
                    //RequiredProductIds = product.RequiredProductIds,
                    AutomaticallyAddRequiredProducts = false,
                    IsDownload = false,
                    DownloadId = 0,
                    UnlimitedDownloads = false,
                    MaxNumberOfDownloads = 0,
                    //DownloadExpirationDays = product.DownloadExpirationDays,
                    DownloadActivationType = 0,
                    HasSampleDownload = false,
                    SampleDownloadId = 0,
                    HasUserAgreement = false,
                    //UserAgreementText = product.UserAgreementText,
                    IsRecurring = false,
                    RecurringCycleLength = 0,
                    RecurringCyclePeriod = 0,
                    RecurringTotalCycles = 0,
                    IsRental = false,
                    RentalPriceLength = 0,
                    RentalPricePeriod = 0,
                    IsShipEnabled = true,
                    IsFreeShipping = false,
                    ShipSeparately = false,
                    AdditionalShippingCharge = 0,
                    DeliveryDateId = 0,
                    IsTaxExempt = false,
                    TaxCategoryId = 0,
                    IsTelecommunicationsOrBroadcastingOrElectronicServices =
                       false,
                    ManageInventoryMethod = 0,
                    ProductAvailabilityRangeId = 0,
                    UseMultipleWarehouses = false,
                    WarehouseId = 0,
                    StockQuantity = 10000,
                    DisplayStockAvailability = false,
                    DisplayStockQuantity = false,
                    MinStockQuantity = 0,
                    LowStockActivityId = 1,
                    NotifyAdminForQuantityBelow = 0,
                    BackorderMode = 0,
                    AllowBackInStockSubscriptions = false,
                    OrderMinimumQuantity = 1,
                    OrderMaximumQuantity = 1000,
                    //AllowedQuantities = "1,2,3,4,5,1",
                    AllowAddingOnlyExistingAttributeCombinations = false,
                    NotReturnable = false,
                    DisableBuyButton = false,
                    DisableWishlistButton = false,
                    AvailableForPreOrder = false,
                    //PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc,
                    CallForPrice = false,
                    Price = item.rate != null ? Convert.ToDecimal(item.rate) : 0,
                    OldPrice = 0,
                    ProductCost = 0,
                    CustomerEntersPrice = false,
                    MinimumCustomerEnteredPrice = 0,
                    MaximumCustomerEnteredPrice = 0,
                    BasepriceEnabled = false,
                    BasepriceAmount = item.rate != null ? Convert.ToDecimal(item.rate) : 0,
                    BasepriceUnitId = 0,
                    BasepriceBaseAmount = 0,
                    BasepriceBaseUnitId = 0,
                    MarkAsNew = false,
                    //MarkAsNewStartDateTimeUtc = product.MarkAsNewStartDateTimeUtc,
                    //MarkAsNewEndDateTimeUtc = product.MarkAsNewEndDateTimeUtc,
                    Weight = 0,
                    //weightUnit
                    Length = 0,
                    Width = 0,
                    Height = 0,
                    //AvailableStartDateTimeUtc = product.AvailableStartDateTimeUtc,
                    //AvailableEndDateTimeUtc = product.AvailableEndDateTimeUtc,
                    DisplayOrder = 0,
                    Published = false,
                    Deleted = item.isClosed,
                    CreatedOnUtc = DateTime.UtcNow,
                    UpdatedOnUtc = DateTime.UtcNow,
                    IdInventoryItem = item.item.id
                };
                //validate search engine name
                _productRepository.Insert(productCopy);

                var categoryproduct = _categoryService.GetProductCategoryById(productCopy.Id);
                if (categoryproduct != null)
                {
                    if (CategoryId != 0)
                    {
                        categoryproduct.ProductId = productCopy.Id;
                        categoryproduct.CategoryId = CategoryId;

                        _categoryService.UpdateProductCategory(categoryproduct);
                    }
                }
                else
                {
                    if (CategoryId != 0)
                    {
                        ProductCategory productCategory = new ProductCategory();
                        productCategory.ProductId = productCopy.Id;
                        productCategory.CategoryId = CategoryId;

                        _categoryService.InsertProductCategory(productCategory);
                    }
                }
                string productName = productCopy.Name.ToString();
                productName = productName.Replace(" ", "-");
                productName = productName.Replace("---", "-");
                productName = productName.Replace("/", "-");
                productName = productName.Replace("(", "-");
                productName = productName.Replace(")", "-");

                var Slug = _urlRecordService.GetActiveSlug(productCopy.Id, productName, 0);
                if (string.IsNullOrEmpty(Slug))
                {
                    var SlugInfo = _urlRecordService.GetBySlug(productName);
                    if (SlugInfo == null)
                        _urlRecordService.SaveSlug(productCopy, productName, 0);
                    else
                    {
                        productName = productName + "_" + productCopy.Id;
                        _urlRecordService.SaveSlug(productCopy, productName, 0);
                    }
                }
                product = productCopy;

            }
            else
            {
                product = InsertProduct(productDto, "inventoryitem");
            }

            return product;
        }
        public Product GetProductByInventoryItem(string inventoryItem)
        {
            if (string.IsNullOrEmpty(inventoryItem))
                throw new ArgumentException("inventoryItem");
            return _productRepository.Table.Where(p => p.IdInventoryItem == inventoryItem && p.Deleted == false).FirstOrDefault();
        }
        #endregion
    }
}
