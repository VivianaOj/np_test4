using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public class NNBoxGeneratorSettings: ISettings
    {

        public decimal InsuranceSurcharge { get; set; }

        public int MarginError { get; set; }

        public int PackingType { get; set; }
    }

    public class ItemProductSummary
    {
        public int Quantity { get; set; }
        public string ProductName { get; set; }
        public int Id { get; set; }
        public decimal Weight { get; set; }

        public string Sku { get; set; }
    }
}
