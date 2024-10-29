using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Common
{
    /// <summary>
    /// ZipCode attribute service
    /// </summary>
    public partial interface IZipCodeService
    {
        /// <summary>
        /// Gets all zipCode attributes
        /// </summary>
        /// <returns>zipCode attributes</returns>
        IList<Zipcode> GetAllZipcode();

        /// <summary>
        /// Zip code validation 
        /// </summary>
        /// <returns>zipCode attributes</returns>
        bool IsValidCode(int countryId, int stateProvinceId, string code);
        AddressValidation GetAddressValidation(Address address);

    }
}
