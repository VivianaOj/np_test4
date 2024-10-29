using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Shipping.NNBoxGenerator
{
    public partial interface IBoxesGeneratorServices 
    {
        void InsertBoxGenerator(GetShippingOptionResponse GetShippingOptions, Customer customer, bool isAsShip, GetShippingOptionRequest items);

        void DeleteBoxGeneratorByUser(Customer customer, IList<ShoppingCartItem> cart);
        void SaveBoxGenerator(Order placedOrder);
        List<AlgorithmPackingResult> GetBoxByOrder(int id, int customerId);
        BSBox GetBoxById(int id);

        List<BSPackedOrder> GetBSPackedOrderList(int id);

        List<BSItemPackedBox> GetBSItemPackedBoxList(int id);
        AlgorithmPackingResult PackingById(int id);
        BSItemPackedBox GetItemPackedBoxId(int id);

        List<BSItemPack> GetBSItemPackList(int id);
    }
}
