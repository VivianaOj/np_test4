using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.NN
{
    public partial class FreightQuote : BaseEntity
    {
        public DateTime RequestDate { get; set; }

        public string Name { get; set; }

        public string Email { get; set; }

        public string Phone { get; set; }

        public string Infomation { get; set; }
        public string Items { get; set; }
        public int BillingAddress_Id { get; set; }

        public int Shippng_Address_Id { get; set; }

        public decimal TotalAmount { get; set; }

    }
}

