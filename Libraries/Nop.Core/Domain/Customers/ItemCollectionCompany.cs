using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Customers
{
    public partial class ItemCollectionCompany : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int CollectionId { get; set; }

        public int CustomerId { get; set; }
        public int CustomerNetsuiteId { get; set; }
    }
}
