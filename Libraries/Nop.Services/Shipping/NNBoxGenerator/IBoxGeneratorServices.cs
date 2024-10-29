using Nop.Core.Domain.Orders;
using Nop.Services.Plugins;
using System;
using System.Collections.Generic;
using System.Text;
using static Nop.Services.Shipping.GetShippingOptionRequest;

namespace Nop.Services.Shipping.NNBoxGenerator
{
    public partial interface IBoxGeneratorServices : IPlugin
    {
        GetShippingOptionRequest GetBoxesPacking(List<PackageItem> cart, int PackingType, int MarginError);

    }
}
