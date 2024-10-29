using Nop.Core.Domain.Directory;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Common
{
    public partial class Zipcode: BaseEntity
    {
        /// <summary>
        /// Gets or sets the Code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// Gets or sets the country identifier
        /// </summary>
        public int? CountryId { get; set; }

        /// <summary>
        /// Gets or sets the StateProvince identifier
        /// </summary>
        public int? StateProvinceId { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public virtual Country Country { get; set; }

        /// <summary>
        /// Gets or sets the country
        /// </summary>
        public virtual StateProvince StateProvince { get; set; }

    }
}
