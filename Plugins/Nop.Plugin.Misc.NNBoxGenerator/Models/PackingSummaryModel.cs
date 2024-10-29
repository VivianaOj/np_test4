using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Services.Shipping;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.Text;
using static Nop.Web.Areas.Admin.Models.Orders.OrderModel;

namespace Nop.Plugin.Misc.NNBoxGenerator.Models
{
    public class PackingSummaryModel : BaseNopModel
    {
        public PackingSummaryModel()
        {
            BoxGeneratorList = new List<BSPackedOrderModel>();
        }

        public BSPackedOrderModel BSPackedOrderModel { get; set; }

        public IList<BSPackedOrderModel> BoxGeneratorList { get; set; }
    }
    public partial class BoxGeneratorList : BaseNopEntityModel
    {
        public BoxGeneratorList()
        {
            BoxGenerator = new List<BoxGenerator>();
            ShippingOptions = new List<ShippingOption>();
        }

        public Customer Customer { get; set; }

        public Order Order { get; set; }
        public Company Company { get; set; }
        public List<BoxGenerator> BoxGenerator { get; set; }
        public List<ShippingOption> ShippingOptions { get; set; }
    }
    public partial class BoxGenerator : BaseNopEntityModel
    {
        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.Name")]
        public string BoxName { get; set; }
        public int Id { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.BoxSize")]
        public string BoxSize { get; set; }
        public int OrderId { get; set; }
        public string OrderInformation { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.BoxTotalWeight")]

        public string BoxTotalWeight { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.FinalBoxTotalWeight")]
        public int FinalBoxTotalWeight { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.BoxContentWeight")]

        public string BoxContentWeight { get; set; }
        public bool Active { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.VolumenBox")]

        public decimal VolumenBox { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.TotalVolumenBox")]

        public decimal TotalVolumenBox { get; set; }

        [NopResourceDisplayName("Nop.Plugin.Misc.NNBoxSelector.Fields.OwnBox")]

        public bool OwnBox { get; set; }
        public string ItemsPacked { get; set; }
        public int quantity { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NNBoxGenerator.Fields.FinalLength")] 
        public decimal Length { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NNBoxGenerator.Fields.FinalWidth")]
        public decimal Width { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NNBoxGenerator.Fields.FinalHeight")]
        public decimal Height { get; set; }

        public List<ItemProductSummary> PackedItems { get; set; }

        public Customer Customer { get; set; }

        public Order Order { get; set; }
        public Company Company { get; set; }

        [NopResourceDisplayName("Admin.DimensionHtml")]
        public string DimensionHtml { get; set; }

        public bool IsAsShip { get; set; }

        [NopResourceDisplayName("Plugins.Misc.NNBoxGenerator.Fields.PercentBoxVolumePacked")]
        public string PercentBoxVolumePacked { get; set; }

    }
    public class BSPackedOrderModel : BaseNopEntityModel
    {
        public int CustomerId { get; set; }
        public int OrderId { get; set; }
        public string OrderNSId { get; set; }
        public string CustomerEmail { get; set; }
        public string CompanyName { get; set; }

        public string Name { get; set; }
        public virtual Order Order { get; set; }
        public virtual Customer Customer { get; set; }
        public string DateCreated { get; set; }
        public string DateUpdated { get; set; }
        public bool Active { get; set; }

        public bool IsCommercial { get; set; }
    }

}
