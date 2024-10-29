using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Catalog
{
    public partial class ItemPricingService : IItemPricingService
    {
        private readonly IRepository<ItemPricing> _itemPricingRepository;

        public ItemPricingService(IRepository<ItemPricing> itemPricingRepository)
        {
            _itemPricingRepository = itemPricingRepository;
        }

        public void DeleteItemPricingByCompanyId(int CompanyId)
        {
            var listItemPricing = _itemPricingRepository.GetListByWhere(s => s.CompanyId == CompanyId);
            _itemPricingRepository.Delete(listItemPricing);
        }

        public void InsertItemPricing(ItemPricing itemPricing)
        {
            _itemPricingRepository.Insert(itemPricing);
        }
    }
}
