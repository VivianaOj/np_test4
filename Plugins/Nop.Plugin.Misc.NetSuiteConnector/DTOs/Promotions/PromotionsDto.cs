using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Promotions
{
    public class PromotionsList
    {
        public List<PromotionsDto> PromotionsDto { get; set; }
    }

    public class PromotionsDto
    {
        public RecordRefDto couponCode { get; set; }
        public RecordRefDto promoCode { get; set; }
    }
}
