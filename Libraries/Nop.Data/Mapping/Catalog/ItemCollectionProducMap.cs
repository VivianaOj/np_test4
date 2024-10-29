using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Catalog
{
    public class ItemCollectionProducMap : NopEntityTypeConfiguration<ItemCollectionProduc>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ItemCollectionProduc> builder)
        {
            builder.ToTable(nameof(ItemCollectionProduc));
            builder.HasKey(itemCollectionProduc => itemCollectionProduc.Id);

            base.Configure(builder);
        }

        #endregion
    }
}
