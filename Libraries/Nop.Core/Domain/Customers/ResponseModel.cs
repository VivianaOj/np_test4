using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class ResponseModel
    {
        public ResponseModel()
        {
            messages = new MesaggesModel();
        }

        public string RefId { get; set; }
        public MesaggesModel messages { get; set; }
        public string customerProfileId { get; set; }
        public string customerPaymentProfileId { get; set; }
        public string[] CustomerPaymentProfileIdList { get; set; }
        public string[] CustomerShippingAddressIdList { get; set; }
        public string[] ValidationDirectResponseList { get; set; }

    }
}
