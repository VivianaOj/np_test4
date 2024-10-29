using System;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;

namespace Nop.Services.Catalog
{
    /// <summary>
    /// Product service
    /// </summary>
    public partial interface IPriceLevelService
    {

        /// <summary>
        /// Inserts a pricelevel
        /// </summary>
        /// <param name="priceLevel">PriceLevel</param>
        void InsertPriceLevel(PriceLevel priceLevel);

        /// <summary>
        /// Update a pricelevel
        /// </summary>
        /// <param name="priceLevel">PriceLevel</param>
        void UpdatePriceLevel(PriceLevel priceLevel);

        /// <summary>
        /// Validate exists a pricelevel
        /// </summary>
        /// <param name="idPriceLevelNetSuite">string</param>
        PriceLevel GetExistsPriceLevel(string idPriceLevelNetSuite);


        void DeletePriceByQtyProductByProductId(int ProductId);
        void InsertPriceByQtyProduct(PriceByQtyProduct priceByQtyProduct);

        PriceByQtyProduct GetPriceByQtyProductAndPriceLevel(int productId, int quantity, int? priceLevelId);
    }
}