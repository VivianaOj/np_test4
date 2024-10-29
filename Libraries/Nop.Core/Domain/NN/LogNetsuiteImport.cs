using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.NN
{

    public partial class LogNetsuiteImport : BaseEntity
    {
        public DateTime DateCreate { get; set; }
        public DateTime LastExecutionDateGeneral { get; set; }

        public string DataFromNetsuite { get; set; }
        public string DataUpdatedNetsuite { get; set; }
        public string ImportName { get; set; }
        public int Type { get; set; }

        public int RegisterId { get; set; }
        public string Message { get; set; }
    }


    
}
