using System;

namespace Nop.Core.Domain.NN
{
    public class PendingDataToSync : BaseEntity
    {
        public int IdItem { get; set; }
        public int Type { get; set; }

        public string ShippingStatus { get; set; }
        public DateTime? FailedDate { get; set; }

        public DateTime? SuccessDate { get; set; }

        public DateTime? UpdateDate { get; set; }
        public bool Synchronized { get; set; }
    }
    public enum ImporterIdentifierType
    {
        ProductImporter = 1,
        CustomerImporter = 2,
        OrderImporter = 3,
        PendingOrderImporter = 4,
        ImageImporter = 5,
        ImportDataFromNetsuite = 6,
        ContactCustomer=7,

        InvoicesSync = 8,
        OrderSendNetsuite = 9,
    }
}
