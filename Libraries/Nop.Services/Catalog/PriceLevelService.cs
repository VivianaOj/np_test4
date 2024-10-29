using System;
using System.Collections.Generic;
using System.Linq;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Data;
using Nop.Core.Data.Extensions;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Stores;
using Nop.Core.Infrastructure;
using Nop.Data;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial class PriceLevelService : IPriceLevelService
    {
        #region Fields
        private readonly IRepository<PriceLevel> _priceLevelRepository;
        private readonly IRepository<PriceByQtyProduct> _priceByQtyProductRepository;
        private readonly ICacheManager _cacheManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IDbContext _dbContext;

        //private readonly CatalogSettings _catalogSettings;
        //private readonly CommonSettings _commonSettings;
        //private readonly IAclService _aclService;
        //
        //private readonly IDataProvider _dataProvider;
        //private readonly IDateRangeService _dateRangeService;
        //
        //
        //private readonly ILanguageService _languageService;
        //private readonly ILocalizationService _localizationService;
        //private readonly IProductAttributeParser _productAttributeParser;
        //private readonly IProductAttributeService _productAttributeService;
        //private readonly IRepository<AclRecord> _aclRepository;
        //private readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        //private readonly IRepository<Product> _productRepository;
        //private readonly IRepository<ProductPicture> _productPictureRepository;
        //private readonly IRepository<ProductReview> _productReviewRepository;
        //private readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        //private readonly IRepository<RelatedProduct> _relatedProductRepository;
        //private readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        //private readonly IRepository<StoreMapping> _storeMappingRepository;
        //private readonly IRepository<TierPrice> _tierPriceRepository;
        //private readonly IStoreMappingService _storeMappingService;
        //private readonly IStoreService _storeService;
        //private readonly IWorkContext _workContext;
        //private readonly LocalizationSettings _localizationSettings;

        #endregion

        #region Ctor

        public PriceLevelService(
            ICacheManager cacheManager,
            IDbContext dbContext,
            IEventPublisher eventPublisher,
            IRepository<PriceLevel> priceLevelRepository,
            IRepository<PriceByQtyProduct> priceByQtyProductRepository
            )
        {
            _priceByQtyProductRepository = priceByQtyProductRepository;
            _priceLevelRepository = priceLevelRepository;
            _cacheManager = cacheManager;
            _dbContext = dbContext;
            _eventPublisher = eventPublisher;
        }

        #endregion

        #region Methods

        #region Products
        /// <summary>
        /// Inserts a pricelevel
        /// </summary>
        /// <param name="priceLevel">PriceLevel</param>
        public void InsertPriceLevel(PriceLevel priceLevel)
        {
            if (priceLevel == null)
                throw new ArgumentNullException(nameof(priceLevel));

            //insert
            _priceLevelRepository.Insert(priceLevel);

            //clear cache
            //_cacheManager.RemoveByPrefix(NopCatalogDefaults.ProductsPrefixCacheKey);

            //event notification
            _eventPublisher.EntityInserted(priceLevel);
        }

        public void UpdatePriceLevel(PriceLevel priceLevel)
        {
            if (priceLevel == null)
                throw new ArgumentNullException(nameof(priceLevel));

            _priceLevelRepository.Update(priceLevel);

            _eventPublisher.EntityUpdated(priceLevel);
        }

        public PriceLevel GetExistsPriceLevel(string idPriceLevelNetSuite)
        {
            if (string.IsNullOrEmpty(idPriceLevelNetSuite))
                throw new ArgumentNullException(nameof(idPriceLevelNetSuite));

            return _priceLevelRepository.GetByWhere(s => s.IdNetSuite.Equals(idPriceLevelNetSuite));
        }

        public void DeletePriceByQtyProductByProductId(int ProductId)
        {
            var prices = _priceByQtyProductRepository.GetListByWhere(s => s.ProductId == ProductId);
            if (prices != null && prices.Count > 0)
                _priceByQtyProductRepository.Delete(prices);
        }

        public void InsertPriceByQtyProduct(PriceByQtyProduct priceByQtyProduct)
        {
            if (priceByQtyProduct == null)
                throw new ArgumentNullException(nameof(priceByQtyProduct));

            //insert
            _priceByQtyProductRepository.Insert(priceByQtyProduct);

            //event notification
            _eventPublisher.EntityInserted(priceByQtyProduct);
        }

        public PriceByQtyProduct GetPriceByQtyProductAndPriceLevel(int productId, int quantity, int? priceLevelId)
        {
            var listPrices = _priceByQtyProductRepository.GetListByWhere(s => s.PriceLevelId == priceLevelId && s.ProductId == productId && quantity >= s.Quantity);
            return listPrices.OrderByDescending(s => s.Quantity).FirstOrDefault();
        }





        #endregion


        #endregion
    }
}