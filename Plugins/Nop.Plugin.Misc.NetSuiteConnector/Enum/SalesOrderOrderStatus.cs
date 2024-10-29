using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Nop.Plugin.Misc.NetSuiteConnector.Enum
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SalesOrderOrderStatus
    {
        [EnumMember(Value = "1")]
        _pendingApproval = 1,

        [EnumMember(Value = "2")]
        _pendingFulfillment = 2,

        [EnumMember(Value = "3")]
        _cancelled = 3,

        [EnumMember(Value = "4")]
        _partiallyFulfilled = 4,

        [EnumMember(Value = "5")]
        _pendingBillingPartFulfilled = 5,

        [EnumMember(Value = "6")]
        _pendingBilling = 6,

        [EnumMember(Value = "7")]
        _fullyBilled = 7,

        [EnumMember(Value = "8")]
        _closed = 8,

        [EnumMember(Value = "9")]
        _undefined = 9
    }

}
