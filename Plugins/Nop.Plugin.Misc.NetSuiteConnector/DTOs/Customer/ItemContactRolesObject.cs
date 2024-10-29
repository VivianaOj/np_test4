using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class ItemContactRolesObject
    {
        #region Fields

        [JsonProperty("contactName")]
        public string ContactName { get; set; }

        [JsonProperty("email")]
        public string Email{ get; set; }

        [JsonProperty("giveAccess")]
        public bool GiveAccess { get; set; }

        #endregion

        #region Related Fields

        [JsonProperty("contact")]
        public RecordRefObject Contact { get; set; }

        [JsonProperty("role")]
        public RecordRefObject Role { get; set; }
        #endregion
    }
}
