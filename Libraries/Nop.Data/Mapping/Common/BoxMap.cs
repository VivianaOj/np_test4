using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;

namespace Nop.Data.Mapping.Common
{
    public partial class BoxMap : NopEntityTypeConfiguration<BSBox>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<BSBox> builder)
        {
           
            builder.ToTable(nameof(BSBox));
            builder.HasKey(deliveryRoutes => deliveryRoutes.Id);

            builder.Property(deliveryRoutes => deliveryRoutes.Name).HasMaxLength(50);
            builder.Property(deliveryRoutes => deliveryRoutes.Height);
            builder.Property(deliveryRoutes => deliveryRoutes.Length);
            builder.Property(deliveryRoutes => deliveryRoutes.Width);
            builder.Property(deliveryRoutes => deliveryRoutes.VolumenBox);
            builder.Property(deliveryRoutes => deliveryRoutes.Active);

            base.Configure(builder);
        }

        #endregion
    }
}
