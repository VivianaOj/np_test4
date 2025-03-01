﻿using System;
using System.Collections.Generic;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;

namespace Nop.Services.Payments
{
    /// <summary>
    /// Represents a payment info holder
    /// </summary>
    [Serializable]
    public partial class ProcessPaymentRequest
    {
        public ProcessPaymentRequest()
        {
            CustomValues = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets or sets a store identifier
        /// </summary>
        public int StoreId { get; set; }

        /// <summary>
        /// Gets or sets a customer identifier
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets an order unique identifier. Used when order is not saved yet (payment gateways that do not redirect a customer to a third-party URL)
        /// </summary>
        public Guid OrderGuid { get; set; }
        /// <summary>
        /// Gets or sets a datetime when "OrderGuid" property was generated (used for security purposes)
        /// </summary>
        public DateTime? OrderGuidGeneratedOnUtc { get; set; }

        /// <summary>
        /// Gets or sets an order total
        /// </summary>
        public decimal OrderTotal { get; set; }

        /// <summary>
        /// /// <summary>
        /// Gets or sets a payment method identifier
        /// </summary>
        /// </summary>
        public string PaymentMethodSystemName { get; set; }

        #region Payment method specific properties 

        /// <summary>
        /// Gets or sets a credit card type (Visa, Master Card, etc...). We leave it empty if not used by a payment gateway
        /// </summary>
        public string CreditCardType { get; set; }

        /// <summary>
        /// Gets or sets a credit card owner name
        /// </summary>
        public string CreditCardName { get; set; }

        /// <summary>
        /// Gets or sets a credit card number
        /// </summary>
        public string CreditCardNumber { get; set; }

        /// <summary>
        /// Gets or sets a credit card expire year
        /// </summary>
        public int CreditCardExpireYear { get; set; }

        /// <summary>
        /// Gets or sets a credit card expire month
        /// </summary>
        public string CreditCardExpireMonth { get; set; }

        /// <summary>
        /// Gets or sets a credit card CVV2 (Card Verification Value)
        /// </summary>
        public string CreditCardCvv2 { get; set; }

        public bool SaveCard { get; set; }

        public string customerProfileId { get; set; }

        public int AddressCustomerProfileId { get; set; }
        public string paymentProfileId { get; set; }

        public string orders { get; set; }
        #endregion

        #region Recurring payments

        /// <summary>
        /// Gets or sets an initial (parent) order identifier if order is recurring
        /// </summary>
        public int InitialOrderId { get; set; }

        /// <summary>
        /// Gets or sets the cycle length
        /// </summary>
        public int RecurringCycleLength { get; set; }

        /// <summary>
        /// Gets or sets the cycle period
        /// </summary>
        public RecurringProductCyclePeriod RecurringCyclePeriod { get; set; }

        /// <summary>
        /// Gets or sets the total cycles
        /// </summary>
        public int RecurringTotalCycles { get; set; }

        #endregion

        /// <summary>
        /// You can store any custom value in this property
        /// </summary>
        public Dictionary<string, object> CustomValues { get; set; }


        public int invoiceId { get; set; }
        public bool IsInvoice { get; set; }

        public decimal ApplyAccountCreditMemo { get; set; }
        public decimal ApplyAccountCustomerDeposite { get; set; }

        public decimal ApplyAccountCustomerPayment { get; set; }

        public Address NewAddress { get; set; }

        public string listTypeDeposite { get; set; }
        public string listTypeMemo { get; set; }
        public string listTypePayment { get; set; }
        public string allCredit { get; set; }
        public string listType { get; set; }
        public decimal CustomerPayment { get; set; }
        public decimal customerDeposite { get; set; }
        public decimal creditmemo { get; set; }

        public decimal discountApply { get; set; }
        public decimal creditCardApply { get; set; }
        public string objectJson { get; set; }

        public decimal Subtotal { get; set; }

        public string InvoicesToPay { get; set; }

        public string CreditsApply { get; set; }

    }

    public class InvoiceModel
    {
        public List<InvoicesToPay> invoicesToPay { get; set; }
    }
    public class InvoicesToPay
    {
        public string OrderId { get; set; }
        public string ValuePay { get; set; }

        public bool IsPay { get; set; }

        public decimal PendingPay { get; set; }
    }

}