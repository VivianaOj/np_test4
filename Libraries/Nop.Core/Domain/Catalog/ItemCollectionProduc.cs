using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Core.Domain.Catalog
{
    public partial class ItemCollectionProduc : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public int CollectionId { get; set; }

        public int ProductId { get; set; }
    }
}
