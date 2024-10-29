using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.DTOs.Customer
{
    public class ContactRolesObject
    {
        #region Fields

        [JsonProperty("totalResults")]
        public int TotalResults { get; set; }

        #endregion

        #region Relationes Fields

        [JsonProperty("items")]
        List<ItemContactRolesObject> Items { get; set; }

        #endregion
    }
}
