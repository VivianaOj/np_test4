using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Catalog;

namespace Nop.Data.Mapping.Catalog
{
    /// <summary>
    /// Represents a product mapping configuration
    /// </summary>
    public partial class PriceByQtyProductMap : NopEntityTypeConfiguration<PriceByQtyProduct>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PriceByQtyProduct> builder)
        {
            builder.ToTable(nameof(PriceByQtyProduct));
            //builder.HasKey(priceByProduct => priceByProduct.Id);

            builder.HasOne(priceByProduct => priceByProduct.Product)
             .WithMany(product => product.PriceByQtyProduct)
             .HasForeignKey(product => product.ProductId)
             .IsRequired();

            base.Configure(builder);
        }

        #endregion
    }
}