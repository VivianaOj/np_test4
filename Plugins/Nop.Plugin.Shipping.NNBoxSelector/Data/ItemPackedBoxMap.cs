using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Shipping.NNBoxSelector.Domain;


namespace Nop.Plugin.Shipping.NNBoxSelector.Data
{
   public class ItemPackedBoxMap : NopEntityTypeConfiguration<BSItemPackedBox>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<BSItemPackedBox> builder)
        {
            builder.ToTable(nameof(BSItemPackedBox));
            builder.HasKey(c => c.Id);
        }

        #endregion
    }
}
