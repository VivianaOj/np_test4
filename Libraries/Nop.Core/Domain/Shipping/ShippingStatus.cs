using System.ComponentModel;

namespace Nop.Core.Domain.Shipping
{
    /// <summary>
    /// Represents the shipping status enumeration
    /// </summary>
    public enum ShippingStatus
    {
        /// <summary>
        /// Shipping not required
        /// </summary>
        ShippingNotRequired = 10,

        /// <summary>
        /// Not yet shipped
        /// </summary>
        NotYetShipped = 20,

        /// <summary>
        /// Partially shipped
        /// </summary>
        PartiallyShipped = 25,

        /// <summary>
        /// Shipped
        /// </summary>
        Shipped = 30,

        /// <summary>
        /// Delivered
        /// </summary>
        Delivered = 40,

        /// <summary>
        /// ArrivalScan 
        /// </summary>
        ArrivalScan = 50,

        /// <summary>
        /// Clearance Completed
        /// </summary>
        ClearanceCompleted = 60,

        /// <summary>
        /// Clearance In Progress
        /// </summary>
        ClearanceInProgress = 70,

        /// <summary>
        /// Departure Scan 
        /// </summary>
        DepartureScan = 80,

        /// <summary>
        /// Destination Scan
        /// </summary>
        DestinationScan = 90,

        /// <summary>
        /// Dropped Off UPS Access Point
        /// </summary>
        DroppedOffUPSAccessPoint = 100,

        /// <summary>
        /// Dropped Off UPS Store
        /// </summary>
        DroppedOffUPSStore = 110,

        /// <summary>
        /// Exception 
        /// </summary>
        Exception = 120,

        /// <summary>
        /// ExportScan
        /// </summary>
        ExportScan = 130,

        /// <summary>
        /// Import Scan
        /// </summary>
        ImportScan = 140,

        /// <summary>
        /// In Transit
        /// </summary>
        InTransit = 150,

        /// <summary>
        /// On Vehicle Delivery
        /// </summary>
        OnVehicleDelivery = 160,

        /// <summary>
        /// Order Processed In Transit to UPS 
        /// </summary>
        OrderProcessedInTransitUPS = 170,

        /// <summary>
        /// Order Processed Ready to UPS
        /// </summary>
        OrderProcessedReadyUPS = 180,

        /// <summary>
        /// Origin Scan
        /// </summary>
        OriginScan = 190,

        /// <summary>
        /// Returned Sender
        /// </summary>
        ReturnedSender = 200,

        /// <summary>
        /// Returning Sender
        /// </summary>
        ReturningSender = 210,

        /// <summary>
        /// Returning Sender On Vehicle Delivery
        /// </summary>
        ReturningSenderOnVehicleDelivery = 220,

        /// <summary>
        /// Shipment Information Voided
        /// </summary>
        ShipmentInformationVoided = 230,

        /// <summary>
        /// Transferred Local Post Office Delivery
        /// </summary>
        TransferredLocalPostOfficeDelivery = 240,

        /// <summary>
        /// Transferred Local Post Office Delivery
        /// </summary>
        Packed = 250,

        /// <summary>
        /// Transferred Local Post Office Delivery
        /// </summary>
        Picked = 260,

        /// <summary>
        /// Transferred Local Post Office Delivery
        /// </summary>
    
        [Description("Ready for Pickup")]
        ReadyforPickup = 270,

        /// <summary>
        /// Transferred Local Post Office Delivery
        /// </summary>
        [Description("Ready for Delivery")]
        ReadyforDelivery = 280,

        /// <summary>
        /// Transferred Local Post Office Delivery
        /// </summary>
        [Description("Ready for UPS Pickup")]
        ReadyforUPSPickup = 290,


    }
}
