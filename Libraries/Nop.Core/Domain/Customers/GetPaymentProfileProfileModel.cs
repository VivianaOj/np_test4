using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public class GetPaymentProfileProfileModel
    {
        public List<GetPaymentProfilePaymentProfilesModel> paymentProfiles { get; set; }
        public string profileType { get; set; }
        public string customerProfileId { get; set; }
        public string merchantCustomerId { get; set; }
        public string description { get; set; }
        public string email { get; set; }

    }
}
