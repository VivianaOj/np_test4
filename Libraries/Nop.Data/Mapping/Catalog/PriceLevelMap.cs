using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Represents a product mapping configuration
    /// </summary>
    public partial class PriceLevelMap : NopEntityTypeConfiguration<PriceLevel>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PriceLevel> builder)
        {
            builder.ToTable(nameof(PriceLevel));
            builder.HasKey(priceLevel => priceLevel.Id);

            base.Configure(builder);
        }

        #endregion
    }
}