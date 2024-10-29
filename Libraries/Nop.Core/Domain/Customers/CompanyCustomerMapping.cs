using Nop.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public partial class CompanyCustomerMapping : BaseEntity
    {
        public int CompanyId { get; set; }
        public int CustomerId { get; set; }

        public virtual Customer Customer { get; set; }

        public virtual Company Company { get; set; }

        public bool DefaultCompany { get; set; }

        public bool Active { get; set; }
    }
}
