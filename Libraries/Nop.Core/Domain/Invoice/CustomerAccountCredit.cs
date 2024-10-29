using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Invoice
{
    public partial class CustomerAccountCredit : BaseEntity
    {
        public decimal AccountCredit { get; set; }

        public DateTime DateSync { get; set; }
        public int CompanyId { get; set; }

        public string Transid { get; set; }

        public  int NetsuiteId { get; set; }

        public DateTime lastModifiedDate { get; set; }

        public decimal TotalApply { get; set; }
        public DateTime DateApplyUpdate { get; set; }

        public int Type { get; set; }

        public bool IsInActive { get; set; }

        public string Name { get; set; }
    }

}
