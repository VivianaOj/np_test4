using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Customers
{
    public class ItemCollectionCompanyMap : NopEntityTypeConfiguration<ItemCollectionCompany>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ItemCollectionCompany> builder)
        {
            builder.ToTable(nameof(ItemCollectionCompany));
            builder.HasKey(itemCollectionCompany => itemCollectionCompany.Id);

            base.Configure(builder);
        }

        #endregion
    }
}
