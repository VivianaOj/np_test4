using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class MesaggesModel
    {
        public string resultCode { get; set; }
        public List<MessageModel> message { get; set; }
    }
}
