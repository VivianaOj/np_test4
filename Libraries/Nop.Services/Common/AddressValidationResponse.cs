using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Common
{
    public class AddressValidationResponse11
    {
        public AddressValidationResponse21 AddressValidationResponse { get; set; }
    }

    public class AddressValidationResponse21
    {
        public Response Response { get; set; }

        public AddressValidationResult AddressValidationResult { get; set; }
    }

    public class AddressValidationResponse2
    {
        public AddressValidationResponse AddressValidationResponse { get; set; }
    }

    public class AddressValidationResponse
    {
        public Response Response { get; set; }
        public List<AddressValidationResult> AddressValidationResult { get; set; }
    }

    

    public class Response
    {
        public TransactionReference TransactionReference { get; set; }

        public string ResponseStatusCode { get; set; }

        public string ResponseStatusDescription { get; set; }

        //public Error Error { get; set; }

    }

    public class TransactionReference
    {
        public string CustomerContext { get; set; }
    }
    public class Error
    {
        public string ErrorSeverity { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorDescription { get; set; }
    }

    public class AddressValidationResult
    {
        public string Rank { get; set; }
        public string Quality { get; set; }
        public string PostalCodeLowEnd { get; set; }
        public string PostalCodeHighEnd { get; set; }
       public AddressUps Address { get; set; }

    }

    public class AddressUps
    {
        public string City { get; set; }
        public string StateProvinceCode { get; set; }
    }
}
