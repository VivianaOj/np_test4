using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Common
{
    public partial interface IDeliveryRoutesService
    {
        List<DeliveryRoutes> GetDeliveryRoute();
    }
}
