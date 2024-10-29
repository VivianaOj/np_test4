using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public partial class CustomerProfileAuthorize: BaseEntity
    {
        /// <summary>
        /// Id Authorize .NET
        /// </summary>
        public  string CustomerProfileId { get; set; }
        /// <summary>
        /// Id Payment Authorize .NET
        /// </summary>
        public string CustomerPaymentProfileList { get; set; }
        /// <summary>
        /// Result Code Api Authorize
        /// </summary>
        public string ResultCode { get; set; }
        /// <summary>
        /// Id Customer NoopComerce
        /// </summary>
        public int CustomerId { get; set; }

      
    }
}
