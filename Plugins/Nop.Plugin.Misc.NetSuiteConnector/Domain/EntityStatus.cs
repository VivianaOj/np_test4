using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class EntityStatus : BaseEntity
    {
        public int EntityStatusId { get; set; }
        public string EntityStatusName { get; set; }
    }
}
