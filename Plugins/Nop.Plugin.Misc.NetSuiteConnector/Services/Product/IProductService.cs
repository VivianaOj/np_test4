using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.NN;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public partial interface IProductService
    {
        void ImportProducts(string LastExecutionDate);
        void ImportProducts(string LastExecutionDate, string idorder = null, string type = null);
        void ImportProducts(string LastExecutionDate, string idorder = null, string type = null, List<PendingDataToSync> listProducts = null);
        Product CreateProductbyNetsuite(OrderItemDto item);
        Product GetProductByInventoryItem(string inventoryItem);

    }
}
