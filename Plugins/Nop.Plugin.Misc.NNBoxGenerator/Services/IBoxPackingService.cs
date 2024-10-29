using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Orders;
using Nop.Services.Shipping;
using System.Collections.Generic;
using static Nop.Services.Shipping.GetShippingOptionRequest;

namespace Nop.Plugin.Misc.NNBoxGenerator.Services
{
    public interface IBoxPackingService
    {
        void InsertBox(BSBox box);
        BSBox GetById(int id);
        void UpdateBSBox(BSBox BSBox);
        IPagedList<BSBox> SearchBoxes(string name, string location, int pageIndex = 0, int pageSize = int.MaxValue);
        void DeleteBoxes(BSBox BSBox);
        List<BSBox> ListBoxes();

        GetShippingOptionRequest GetShippingOptions(List<PackageItem> cart, int PackingType, int MarginError);

        List<AlgorithmPackingResult> GetBoxByOrder();
        AlgorithmPackingResult PackingById(int id);
        AlgorithmPackingResult PackingOrderById(int id);

        BSItemPackedBox GetItemPackedBoxId(int id);
        List<BSItemPackedBox> GetItemPackedBoxIdList(int id);
        IPagedList<BSItemPackedBox> ListPacking(string name, int OrderId, int pageIndex = 0, int pageSize = int.MaxValue);
        void UpdateBSItemPackedBox(BSItemPackedBox BSBox);
        List<BSItemPackedBox> GetBSItemPackedBoxList(int id);


        List<BSPackedOrder> GetOrderSummaryList();
        void UpdateBSPackedOrder(BSPackedOrder ItemPackedBox);
        BSPackedOrder GetBSPackedOrderById(int id);
        List<BSPackedOrder> BSPackedOrderById(int id);
        IPagedList<BSPackedOrder> SearchPacking(string name, int OrderId, int pageIndex = 0, int pageSize = int.MaxValue);

        List<BSItemPack> GetBSItemPackList(int id);
    }
}
