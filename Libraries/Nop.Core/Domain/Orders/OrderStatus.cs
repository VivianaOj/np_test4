using System.ComponentModel;

namespace Nop.Core.Domain.Orders
{
    /// <summary>
    /// Represents an order status enumeration
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// Pending
        /// </summary>
        [Description("Order Processing")]
        Pending = 10,

        /// <summary>
        /// Order Processing OrderConfirmed peding fullfiltment
        /// </summary>
        [Description("Order Confirmed - Pending fulfillment")]
        Processing = 20,

       
        /// <summary>
        /// Complete
        /// </summary>
        [Description("Completed")]
        Complete = 30,

        /// <summary>
        /// Cancelled
        /// </summary>
        [Description("Cancelled")]
        Cancelled = 40,

        /// <summary>
        /// Ready for Pickup
        /// </summary>
        [Description("Ready for Pickup")]
        ReadyPickup = 60,

        /// <summary>
        /// Ready for Delivery
        /// </summary>
        [Description("Ready for Delivery")]
        ReadyDelivery = 70,

        /// <summary>
        /// ReadyUps pickup
        /// </summary>
        [Description("Ready for UPS Pickup")]
        ReadyUps = 80,

        /// <summary>
        /// Cancelled
        /// </summary>
        [Description("Completed and Billed")]
        CompletedBilled = 90,

        /// <summary>
        /// Order Processing OrderConfirmed peding fullfiltment
        /// </summary>
        [Description("Order Confirmed - Billing/Partially Fulfilled")]
        BillingPartiallyFulfilled = 100,

        /// <summary>
        /// Order Processing OrderConfirmed peding fullfiltment
        /// </summary>
        [Description("Order Confirmed - Pending Billing/Partially Fulfilled")]
        PendingPartiallyFulfilled = 110,

        /// <summary>
        /// Order Processing OrderConfirmed peding fullfiltment
        /// </summary>
        [Description("Order Confirmed - Pending Billing")]
        PartiallyFulfilled = 120,

        /// <summary>
        /// Order Processing OrderConfirmed peding fullfiltment
        /// </summary>
        [Description("Closed")]
        Closed = 130,

    }
}
