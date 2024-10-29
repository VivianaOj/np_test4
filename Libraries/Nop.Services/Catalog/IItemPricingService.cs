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
    public partial interface IItemPricingService
    {

        void DeleteItemPricingByCompanyId(int CompanyId);
        void InsertItemPricing(ItemPricing itemPricing);

    }
}