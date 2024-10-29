using Nop.Core;
using Nop.Core.Data;
using Nop.Plugin.Shipping.NNDelivery.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Shipping.NNDelivery.Services
{
    public class DeliveryRoutesService : IDeliveryRoutesService
    {
        #region Fields

        private readonly IRepository<DeliveryRoutes> _repository;

        #endregion

        #region Ctor

        public DeliveryRoutesService(IRepository<DeliveryRoutes> repository)
        {
            _repository = repository;
        }

        #endregion

        #region Methods

        public void DeleteDeliveryRoute(DeliveryRoutes deliveryRoutes)
        {
            if (deliveryRoutes == null)
                throw new ArgumentNullException(nameof(deliveryRoutes));

            _repository.Delete(deliveryRoutes);
        }

        public DeliveryRoutes GetById(int id)
        {
            if (id == 0)
                return null;

            return _repository.GetById(id);
        }

        public DeliveryRoutes GetByLocation(string location)
        {
            if (string.IsNullOrEmpty(location))
                return null;

            return _repository.Table.FirstOrDefault(r => r.Name.ToLower() == location && r.Available);
        }

        public DeliveryRoutes GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return _repository.Table.FirstOrDefault(r => r.Name == name && r.Available);
        }

        public List<DeliveryRoutes> GetDeliveryRoutes()
        {
            return _repository.Table.ToList();
        }

        public void InsertDeliveryRoute(DeliveryRoutes deliveryRoutes)
        {
            if (deliveryRoutes == null)
                throw new ArgumentNullException(nameof(deliveryRoutes));

            _repository.Insert(deliveryRoutes);
        }

        public IPagedList<DeliveryRoutes> SearchDeliveryRoutes(string name, string location, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var deliveryRoutes = _repository.Table;

            if (!string.IsNullOrEmpty(name))
                deliveryRoutes = deliveryRoutes.Where(d => d.Name.Contains(name));

            if (!string.IsNullOrEmpty(location))
                deliveryRoutes = deliveryRoutes.Where(d => d.Location.Contains(location));

            return new PagedList<DeliveryRoutes>(deliveryRoutes, pageIndex, pageSize);
        }

        public void UpdateDeliveryRoute(DeliveryRoutes deliveryRoutes)
        {
            if (deliveryRoutes == null)
                throw new ArgumentNullException(nameof(deliveryRoutes));

            _repository.Update(deliveryRoutes);
        }

        #endregion
    }
}