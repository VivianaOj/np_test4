using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class CustomerDto
    {
        #region Fields

        [JsonProperty("companyName")]
        public string CompanyName { get; set; }

        [JsonProperty("balance")]
        public decimal Balance { get; set; }

        [JsonProperty("consolBalance")]
        public decimal ConsoleBalance { get; set; }

        [JsonProperty("consolDepositBalance")]
        public decimal ConsoleDepositBalance { get; set; }

        [JsonProperty("consolOverdueBalance")]
        public decimal ConsoleOverdueBalance { get; set; }

        [JsonProperty("consolUnbilledOrders")]
        public decimal ConsolUnbilledOrders { get; set; }

        [JsonProperty("creditLimit")]
        public decimal CreditLimit { get; set; }

        [JsonProperty("custentity10")]
        public string EmailForCustomerBiling { get; set; }

        [JsonProperty("custentity11")]
        public bool AccountInNyNCollectionsProcess { get; set; }

        [JsonProperty("custentity51")]
        public string BillingEmailBackup { get; set; }

        [JsonProperty("custentity57")]
        public decimal OverdueBalance2 { get; set; }

        [JsonProperty("custentity61")]
        public string  LastPaymentDate { get; set; }

        [JsonProperty("custentity62")]
        public decimal LastPaymenAmount { get; set; }

        [JsonProperty("custentity66")]
        public bool TermsEmailSent { get; set; }

        [JsonProperty("custentity69")]
        public bool ACHPaymentProccesing { get; set; }

        [JsonProperty("custentity7")]
        public string FranchiseNameNumber { get; set; }

        [JsonProperty("custentity70")]
        public bool PrepaidCustomer { get; set; }

        [JsonProperty("custentity71")]
        public bool LimitedAccess { get; set; }

        [JsonProperty("custentity8")]
        public string  USDOT { get; set; }

        [JsonProperty("custentity80")]
        public bool Sent2021Candelar { get; set; }

        [JsonProperty("custentity88")]
        public bool IncludeInRunTracker { get; set; }

        [JsonProperty("custentity89")]
        public bool IsParent { get; set; }

        [JsonProperty("custentity_2663_customer_refund")]
        public bool EFTCustomerRefundPayment { get; set; }

        [JsonProperty("custentity_2663_direct_debit")]
        public bool DirectDebit { get; set; }

        [JsonProperty("custentity_date_lsa")]
        public string LastSalesActivity { get; set; }

        [JsonProperty("custentity_esc_last_modified_date")]
        public string  CuentityEscLastModifiedDate { get; set; }

        [JsonProperty("custentity_link_lsa")]
        public string LSALink { get; set; }

        [JsonProperty("custentity_link_name_lsa")]
        public string LSALinkName { get; set; }

        [JsonProperty("custentity_naw_trans_need_approval")]
        public bool TransactionNeedApproval { get; set; }

        [JsonProperty("custentity_nn_cust_storage")]
        public bool Customerstorage { get; set; }

        [JsonProperty("custentity_po_number_req")]
        public bool PORequired { get; set; }

        [JsonProperty("custentity_run_cc_due")]
        public bool RunCCWhenDue { get; set; }

        [JsonProperty("custentityrun_cc_now")]
        public bool RunCCNow { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("defaultAddress")]
        public string DefaultAddress { get; set; }

        [JsonProperty("depositBalance")]
        public decimal DepositBalance { get; set; }

        [JsonProperty("displaySymbol")]
        public string DisplaySymbol { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("emailTransactions")]
        public bool EmailTransactions { get; set; }

        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        [JsonProperty("faxTransactions")]
        public bool FaxTransactions { get; set; }

        [JsonProperty("giveAccess")]
        public bool GiveAccess { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("isBudgetApproved")]
        public bool IsBudgetApproved { get; set; }

        [JsonProperty("isInactive")]
        public bool IsInactive { get; set; }

        [JsonProperty("isPerson")]
        public bool IsPerson { get; set; }

        [JsonProperty("lastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }

        [JsonProperty("overdueBalance")]
        public decimal OverdueBalance { get; set; }

        [JsonProperty("overrideCurrencyFormat")]
        public bool OverrideCurrencyFormat { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("printTransactions")]
        public bool PrintTransactions { get; set; }

        [JsonProperty("shipComplete")]
        public bool ShipComplete { get; set; }

        [JsonProperty("startDate")]
        public string StartDate { get; set; }

        [JsonProperty("unbilledOrders")]
        public decimal UnbilledOrders { get; set; }

        [JsonProperty("custentity_item_collection")]
        public DefaultObject ItemCollection { get; set; }

        [JsonProperty("priceLevel")]
        public DefaultObject PriceLevel { get; set; }

        [JsonProperty("parent")]
        public DefaultObject parent { get; set; }

        [JsonProperty("custentity_tj_exempt_customer")]
        public bool custentity_tj_exempt_customer { get; set; }

        [JsonProperty("custentity_tj_exempt_customer_states")]
        public DefaultObject custentity_tj_exempt_customer_states { get; set; }
        #endregion

        #region Relationed Fiels

        [JsonProperty("addressBook")]
        public AddressBookObject AddressBook { get; set; }

        [JsonProperty("alcoholRecipientType")]
        public RecordRefObject alcoholRecipientType { get; set; }

        [JsonProperty("contactRoles")]
        public ContactRolesObject ContactRolesObject { get; set; }

        [JsonProperty("custentity22")]
        public EmployeeObject Employee { get; set; }

        [JsonProperty("custentity6")]
        public DefaultObject MustReadNotes { get; set; }

        [JsonProperty("custentity68")]
        public DefaultObject MultipleLocationSelect { get; set; }

        [JsonProperty("custentity_nn_dock")]
        public DefaultObject DeliveryInformation { get; set; }

        [JsonProperty("custentity_primary_location_field")]
        public DefaultObject PrimaryLocation { get; set; }

        [JsonProperty("emailPreference")]

        public RecordRefObject EmailPreference { get; set; }

        [JsonProperty("entityStatus")]
        public DefaultObject EntityStatus { get; set; }
        
        [JsonProperty("globalSubscriptionStatus")]
        public RecordRefObject GlobalSubscriptionStatus { get; set; }

        [JsonProperty("groupPricing")]
        public DefaultObject GroupPricing { get; set; }

        [JsonProperty("itemPricing")]
        public DefaultObject ItemPricing { get; set; }

        [JsonProperty("receivablesAccount")]
        public DefaultObject ReceivablesAccount { get; set; }

        [JsonProperty("salesRep")]
        public DefaultObject SalesRep { get; set; }

        [JsonProperty("shippingCarrier")]
        public RecordRefObject ShippingCarrier { get; set; }

        [JsonProperty("subscriptionMessageHistory")]
        public DefaultObject SubscriptionMessageHistory { get; set; }

        [JsonProperty("subscriptions")]
        public DefaultObject Subscriptions { get; set; }

        [JsonProperty("subsidiary")]
        public DefaultObject Subsidiary { get; set; }

        [JsonProperty("symbolPlacement")]
        public RecordRefObject SymbolPlacement { get; set; }

        [JsonProperty("terms")]
        public DefaultObject Terms { get; set; }


        [JsonProperty("custentity_rt_route")]
        public DefaultObject Deliveryroute { get; set; }

        #endregion

    }
}
