using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Shipping.NNDelivery.Domain;

namespace Nop.Plugin.Shipping.NNDelivery.Data
{
    public class DeliveryRoutesMap : NopEntityTypeConfiguration<DeliveryRoutes>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<DeliveryRoutes> builder)
        {
            builder.ToTable(nameof(DeliveryRoutes));
            builder.HasKey(c => c.Id);
        }

        #endregion
    }
}
