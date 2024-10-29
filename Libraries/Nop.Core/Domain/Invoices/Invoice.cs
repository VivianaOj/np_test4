using Nop.Core.Domain.Payments;
using System;

namespace Nop.Core.Domain.Invoices
{
    public partial class Invoice : BaseEntity
    {
        public DateTime CreatedDate { get; set; }

        public string PostingPeriod { get; set; }

        public string Location { get; set; }

        public string PONumber { get; set; }

        public string PlacedByName { get; set; }

        public int ShippingId { get; set; }

        public int SaleOrderId { get; set; }

        public int CustomerId { get; set; }

        public decimal Subtotal { get; set; }

        public decimal DiscountItem { get; set; }

        public decimal TaxTotal { get; set; }

        public decimal ShippingCost { get; set; }

        public decimal Total { get; set; }

        public decimal AmountDue { get; set; }

        public string InvoiceNo { get; set; }

        public string Status { get; set; }

        public string StatusName { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public int InvoiceNetSuiteId { get; set; }

        public int CompanyId { get; set; }
        public DateTime duedate { get; set; }
        public decimal foreigntotal { get; set; }
        public decimal foreignamountpaid { get; set; }
        public decimal foreignamountunpaid { get; set; }

        public bool StatusPaymentNP { get; set; }


        

        public int PaymentStatusId { get; set; }
        /// <summary>
        /// Gets or sets the card type
        /// </summary>
        public string CardType { get; set; }

        /// <summary>
        /// Gets or sets the card name
        /// </summary>
        public string CardName { get; set; }

        /// <summary>
        /// Gets or sets the card number
        /// </summary>
        public string CardNumber { get; set; }

        /// <summary>
        /// Gets or sets the masked credit card number
        /// </summary>
        public string MaskedCreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets the card CVV2
        /// </summary>
        public string CardCvv2 { get; set; }

        /// <summary>
        /// Gets or sets the card expiration month
        /// </summary>
        public string CardExpirationMonth { get; set; }

        /// <summary>
        /// Gets or sets the card expiration year
        /// </summary>
        public string CardExpirationYear { get; set; }

        /// <summary>
        /// Gets or sets the payment method system name
        /// </summary>
        public string PaymentMethodSystemName { get; set; }
    }
}
