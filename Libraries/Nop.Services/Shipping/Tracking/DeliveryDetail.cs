namespace Nop.Services.Shipping.Tracking
{
    /// <summary>
    /// Delivery detail
    /// </summary>
    public partial class DeliveryDetail
    {
        /// <summary>
        /// Date 
        /// </summary>
        public string Date { get; set; }

        /// <summary>
        /// Delivery Type
        /// </summary>
        public DeliveryType Type { get; set; }
    }

    /// <summary>
    /// Delivery type
    /// </summary>
    public partial class DeliveryType
    {
        /// <summary>
        /// Code Type 
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Description Type
        /// </summary>
        public string Description { get; set; }
    }

}
