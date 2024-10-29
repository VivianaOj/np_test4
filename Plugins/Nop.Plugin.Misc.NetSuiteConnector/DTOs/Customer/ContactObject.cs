using Newtonsoft.Json;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class ContactObject
    {
        #region Fields

       
        [JsonProperty("custentity_esc_last_modified_date")]
        public DateTime LastModifiedDate { get; set; }

        [JsonProperty("dateCreated")]
        public DateTime dateCreated { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("entityId")]
        public string EntityId { get; set; }

        [JsonProperty("firstName")]
        public string FirstName { get; set; }

        [JsonProperty("lastName")]
        public string LastName { get; set; } 

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("IsInactive")]
        public bool IsInactive { get; set; }

        [JsonProperty("custentity_website_access")]
        public bool custentity_website_access { get; set; }

        [JsonProperty("isPrivate")]
        public bool IsPrivate { get; set; }

        [JsonProperty("mobilePhone")]
        public string MobilePhone { get; set; }

        [JsonProperty("officePhone")]
        public string OfficePhone { get; set; }

        [JsonProperty("owner")]
        public int Owner { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        #endregion


        #region Related Fields

        

        [JsonProperty("category")]
        public EnityLinkRelObject Category { get; set; }

        [JsonProperty("company")]
        public EntityLinkRefObject Company { get; set; }

        [JsonProperty("customForm")]
        public EntityRefObject CustomForm { get; set; }

        [JsonProperty("globalSubscriptionStatus")]
        public EntityRefObject globalSubscriptionStatus { get; set; }

        [JsonProperty("subsidiary")]
        public DefaultObject Subsidiary { get; set; }
        #endregion
    }


    public class ContactUpdateNetsuite
    {
        public bool custentity_registered_online { get; set; }
    }
}

