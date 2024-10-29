using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Shipping.NNBoxSelector.Domain;


namespace Nop.Plugin.Shipping.NNBoxSelector.Data
{
    public class BoxMap : NopEntityTypeConfiguration<BSBox>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<BSBox> builder)
        {
            builder.ToTable(nameof(BSBox));
            builder.HasKey(c => c.Id);
        }

        #endregion
    }
}
