using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class ResponseGetCustomerPaymentProfileModel
    {

        public GetPaymentProfileProfileModel profile { get; set; }
        public MesaggesModel messages { get; set; }
    }
}
