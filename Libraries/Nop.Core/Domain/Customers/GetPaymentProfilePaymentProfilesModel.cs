using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class GetPaymentProfilePaymentProfilesModel
    {
        public string customerPaymentProfileId { get; set; }
        public GetPaymentProfilePaymentModel payment { get; set; }
        public BillToModel billTo { get; set; }
        public string customerType { get; set; }

    }
}
