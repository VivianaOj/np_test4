using Nop.Core;
using Nop.Core.Domain.Common;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.NNBoxSelector.Services
{
    public interface IBoxPackingService
    {
        void InsertBox(BSBox box);
        BSBox GetById(int id);
        void UpdateDeliveryRoute(BSBox BSBox);
        IPagedList<BSBox> SearchBoxes(string name, string location, int pageIndex = 0, int pageSize = int.MaxValue);
        void DeleteBoxes(BSBox BSBox);
        List<BSBox> ListBoxes();
    }
}
