using Nop.Plugin.Misc.NetSuiteConnector.DTOs.RecordRef;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.GiftCertRedemption
{
    public class GiftCertRedemptionList
    {
        public List<GiftCertRedemptionDto> GiftCertRedemptionListDto { get; set; }
    }

    public class GiftCertRedemptionDto
    {
        public RecordRefDto authCode { get; set; }
        public double authCodeAmtRemaining { get; set; }
        public double authCodeApplied { get; set; }
        public double giftCertAvailable { get; set; }
    }
}
