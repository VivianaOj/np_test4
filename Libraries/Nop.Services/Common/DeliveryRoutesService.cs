using Nop.Core.Data;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nop.Services.Common
{
    public partial class DeliveryRoutesService: IDeliveryRoutesService
    {
        #region Fields
        private readonly IRepository<DeliveryRoutes> _deliveryRoutesRepository;

        #endregion

        #region Ctor
        public DeliveryRoutesService(IRepository<DeliveryRoutes> deliveryRoutesRepository)
        {
            _deliveryRoutesRepository = deliveryRoutesRepository;
        }




        #endregion

        #region Methods

        public List<DeliveryRoutes> GetDeliveryRoute()
        {
            var query = _deliveryRoutesRepository.Table.Where(x => x.Available).ToList();
            List<DeliveryRoutes> deliveryRoutes = new List<DeliveryRoutes>();
            foreach (var item in query)
            {
                deliveryRoutes.Add(new DeliveryRoutes
                {
                    Location = item.Location,
                    Name = item.Name,
                    Minimum = item.Minimum,
                    Available = item.Available
                });

            }

            return deliveryRoutes;
        }

        #endregion
    }
}
