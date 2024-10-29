using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Catalog
{
    public class ItemPricingMap : NopEntityTypeConfiguration<ItemPricing>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ItemPricing> builder)
        {
            builder.ToTable(nameof(ItemPricing));
            builder.HasKey(itemPricing => itemPricing.Id);

            builder.HasOne(itemPricing => itemPricing.Company)
                .WithMany(company => company.ItemsPricing)
                .HasForeignKey(itemPricing => itemPricing.CompanyId)
                .IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}
