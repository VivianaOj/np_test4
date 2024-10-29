using System;
using Nop.Core;

namespace Nop.Plugin.Misc.NetSuiteConnector.Domain
{
    public class NetSuiteImporter : BaseEntity
    {
        public string Name { get; set; }

        public string ControllerName { get; set; }

        public string ActionName { get; set; }

        public DateTime? LastExecutionDate { get; set; }
    }
}
