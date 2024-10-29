using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class EmployeeObject
    {
        #region Fields

        [JsonProperty("birthdate")]
        public string BirthDate { get; set; }

        [JsonProperty("btemplate")]
        public string Btemplate { get; set; }

        [JsonProperty("concurrentwebservicesuser")]
        public bool ConcurrentWebServiceUser { get; set; }

        [JsonProperty("custentity1")]
        public string ClassCode { get; set; }

        [JsonProperty("custentity12")]
        public bool EmployeeHandbook { get; set; }

        [JsonProperty("custentity13")]
        public bool CopyOfDriverLicense { get; set; }

        [JsonProperty("custentity14")]
        public bool CopyOfSSCard { get; set; }

        [JsonProperty("custentity15")]
        public bool Application { get; set; }

        [JsonProperty("custentity16")]
        public bool BackgroundConsentForm { get; set; }

        [JsonProperty("custentity17")]
        public bool W4 { get; set; }

        [JsonProperty("custentity18")]
        public bool DirectDeposit { get; set; }

        [JsonProperty("custentity19")]
        public bool I9 { get; set; }

        [JsonProperty("custentity2")]
        public string EIN { get; set; }

        [JsonProperty("custentity20")]
        public bool StateTaxForm { get; set; }

        [JsonProperty("custentity23")]
        public string OnboardingFilesLink { get; set; }

        [JsonProperty("custentity3")]
        public string ForkLiftCertificationDate { get; set; }

        [JsonProperty("custentity4")]
        public string ForkLiftCertExpDate { get; set; }

        [JsonProperty("custentity42")]
        public decimal CellphoneReimbursement { get; set; }

        [JsonProperty("custentity43")]
        public string Last4digits { get; set; }

        [JsonProperty("custentity47")]
        public bool COBRA { get; set; }

        [JsonProperty("custentity5")]
        public bool BackGroundCheckRound { get; set; }

        [JsonProperty("custentity75")]
        public bool Driver { get; set; }

        [JsonProperty("custentity82")]
        public decimal CarReimbursement { get; set; }

        [JsonProperty("custentity83")]
        public bool Officer { get; set; }

        [JsonProperty("custentity84")]
        public bool CDLDriver { get; set; }

        [JsonProperty("custentity_2663_payment_method")]
        public bool EFTBillPayment { get; set; }

        [JsonProperty("custentity_esc_last_modified_date")]
        public string CuestintyEscLastModifiedDate { get; set; }

        [JsonProperty("custentity_restrictexpensify")]
        public bool RestrictAccesToExpesify { get; set; }

        [JsonProperty("custentity_weeks_worked")]
        public decimal WeeksWorked { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime DateCreated { get; set; }

        [JsonProperty("defaultAddress")]
        public string DefaultAddress { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("empcenterqty")]
        public string EmpCenterQuantity { get; set; }

        [JsonProperty("empcenterqtymax")]
        public string empCenterQuantityMax { get; set; }

        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        [JsonProperty("externalId")]
        public string ExternalId { get; set; }

        [JsonProperty("firstName")]
        public string firstName { get; set; }

        [JsonProperty("fulluserqty")]
        public string FullUserQuantity { get; set; }

        [JsonProperty("fulluserqtymax")]
        public string FullUserQuantityMax { get; set; }

        [JsonProperty("giveAccess")]
        public bool GiveAcces { get; set; }

        [JsonProperty("hiredate")]
        public string HireDate { get; set; }

        [JsonProperty("i9verified")]
        public bool I9Verified { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("initials")]
        public string Initials { get; set; }

        [JsonProperty("isempcenterqtyenforced")]
        public string IsempCenterQuantityEnforced { get; set; }

        [JsonProperty("isfulluserqtyenforced")]
        public string IsFullUserQuantityEnforced { get; set; }

        [JsonProperty("isInactive")]
        public bool IsInactive { get; set; }

        [JsonProperty("isretailuserqtyenforced")]
        public string IsRetailUserQuantityEnforced { get; set; }

        [JsonProperty("issalesrep")]
        public bool IsSalesRep { get; set; }

        [JsonProperty("issupportrep")]
        public bool IsSupportRep { get; set; }

        [JsonProperty("lastModifiedDate")]
        public DateTime LastModifiedDate { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("requirePwdChange")]
        public bool RequirePwdChange { get; set; }

        [JsonProperty("retailuserqty")]
        public string RetailUserQty { get; set; }

        [JsonProperty("retailuserqtymax")]
        public string RetailUserQtyMax { get; set; }

        [JsonProperty("sendEmail")]
        public bool SendEmail { get; set; }

        [JsonProperty("socialsecuritynumber")]
        public string SocialSecurityNumber { get; set; }

        [JsonProperty("terminationbydeath")]
        public bool TerminationByDeath { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("wasempcenterhasaccess")]
        public string WasEmpCenterHasAccess { get; set; }

        [JsonProperty("wasfulluserhasaccess")]
        public string WasFullUserHasAccess { get; set; }

        [JsonProperty("wasinactive")]
        public string WasInactive { get; set; }

        [JsonProperty("wasretailuserhasaccess")]
        public string WasRetailUserHasAccess { get; set; }

        #endregion



        #region RelatedFields

        [JsonProperty("addressBook")]
        public AddressBookObject AddressBook { get; set; }


        [JsonProperty("corporatecards")]
        public DefaultObject CorporateCards { get; set; }

        [JsonProperty("custentity25")]
        public DefaultObject Vacation { get; set; }

        [JsonProperty("custentity26")]
        public DefaultObject CorporateCreditCard { get; set; }

        [JsonProperty("custentity27")]
        public DefaultObject DistractedDriving { get; set; }

        [JsonProperty("custentity28")]
        public DefaultObject ForkLiftSWP { get; set; }

        [JsonProperty("custentity29")]
        public DefaultObject CellPhone { get; set; }

        [JsonProperty("custentity30")]
        public DefaultObject UniformPhoneAnswering  { get; set; }

        [JsonProperty("custentity31")]
        public DefaultObject AccountsReceivable { get; set; }

        [JsonProperty("custentity32")]
        public DefaultObject RunBackgroundCheck{ get; set; }

        [JsonProperty("custentity33")]
        public DefaultObject ReportHireToVerify { get; set; }

        [JsonProperty("custentity34")]
        public DefaultObject SetUpNovatime { get; set; }

        [JsonProperty("custentity35")]
        public DefaultObject SetUpEmail { get; set; }

        [JsonProperty("custentity36")]
        public DefaultObject AddGoodhireReportToDrive { get; set; }

        [JsonProperty("custentity37")]
        public DefaultObject SetUpGoogleDriveFile { get; set; }

        [JsonProperty("custentity38")]
        public DefaultObject OrderCreditCard { get; set; }

        [JsonProperty("custentity39")]
        public DefaultObject SetUpCapitalOneOnlineAccount { get; set; }

        [JsonProperty("custentity40")]
        public DefaultObject OrderBussinesCards { get; set; }

        [JsonProperty("custentity41")]
        public DefaultObject RegisterWithCounty { get; set; }

        [JsonProperty("custentity_2663_eft_file_format")]
        public DefaultObject EFTFileFormat { get; set; }

        [JsonProperty("customForm")]
        public RecordRefObject CustomForm { get; set; }

        [JsonProperty("department")]
        public RecordRefObject Department { get; set; }

        [JsonProperty("effectivedatemode")]
        public RecordRefObject EffetiveDateMode { get; set; }

        [JsonProperty("employeestatus")]
        public RecordRefObject EmployeeStatus { get; set; }

        [JsonProperty("employeetype")]
        public RecordRefObject EmployeeType { get; set; }

        [JsonProperty("empperms")]

        public RecordRefObject Empperms { get; set; }

        [JsonProperty("gender")]
        public RecordRefObject Gender { get; set; }

        [JsonProperty("globalSubscriptionStatus")]
        public RecordRefObject GlobalSubscriptionStatus { get; set; }

        [JsonProperty("location")]
        public RecordRefObject Location { get; set; }

        [JsonProperty("roles")]
        public DefaultObject Roles { get; set; }

        [JsonProperty("subscriptionMessageHistory")]
        public DefaultObject SubscriptionMessageHistory { get; set; }

        [JsonProperty("subscriptions")]
        public DefaultObject Subscriptions { get; set; }

        [JsonProperty("subsidiary")]
        public DefaultObject Subsidiary { get; set; }

        [JsonProperty("supervisor")]
        public DefaultObject Supervisor { get; set; }

        [JsonProperty("usetimedata")]
        public RecordRefObject UseTimeData { get; set; }

        [JsonProperty("workCalendar")]
        public DefaultObject WorkCalendar { get; set; }

        #endregion


    }
}
