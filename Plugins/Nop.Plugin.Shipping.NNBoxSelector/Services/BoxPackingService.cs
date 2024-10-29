using Nop.Core;
using Nop.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using BSBox = Nop.Core.Domain.Common.BSBox;

namespace Nop.Plugin.Shipping.NNBoxSelector.Services
{
    public class BoxPackingService : IBoxPackingService
    {
        #region Fields

        private readonly IRepository<BSBox> _repository;

        #endregion

        #region Ctor

        public BoxPackingService(IRepository<BSBox> repository)
        {
            _repository = repository;
        }

        #endregion

        public void InsertBox(BSBox box)
        {
            if (box == null)
                throw new ArgumentNullException(nameof(box));

            _repository.Insert(box);
        }

        public IPagedList<BSBox> SearchBoxes(string name, string location, int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var deliveryRoutes = _repository.Table;

            if (!string.IsNullOrEmpty(name))
                deliveryRoutes = deliveryRoutes.Where(d => d.Name.Contains(name));

           
            return new PagedList<BSBox>(deliveryRoutes, pageIndex, pageSize);
        }

        public BSBox GetById(int id)
        {
            if (id == 0)
                return null;

            return _repository.GetById(id);
        }

        public void UpdateDeliveryRoute(BSBox BSBox)
        {
            if (BSBox == null)
                throw new ArgumentNullException(nameof(BSBox));

            _repository.Update(BSBox);
        }

        public void DeleteBoxes(BSBox BSBox)
        {
            if (BSBox == null)
                throw new ArgumentNullException(nameof(BSBox));

            _repository.Delete(BSBox);
        }

        public List<BSBox> ListBoxes()
        {
            return _repository.Table.ToList();
        }

    }
}
