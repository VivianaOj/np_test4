using Nop.Core;
using Nop.Plugin.Shipping.NNDelivery.Domain;
using System.Collections.Generic;

namespace Nop.Plugin.Shipping.NNDelivery.Services
{
    public interface IDeliveryRoutesService
    {
        DeliveryRoutes GetById(int id);

        DeliveryRoutes GetByName(string name);

        DeliveryRoutes GetByLocation(string location);

        List<DeliveryRoutes> GetDeliveryRoutes();

        void UpdateDeliveryRoute(DeliveryRoutes deliveryRoutes);

        void InsertDeliveryRoute(DeliveryRoutes deliveryRoutes);

        void DeleteDeliveryRoute(DeliveryRoutes deliveryRoutes);

        IPagedList<DeliveryRoutes> SearchDeliveryRoutes(string name, string location, int pageIndex = 0, int pageSize = int.MaxValue);
    }
}
