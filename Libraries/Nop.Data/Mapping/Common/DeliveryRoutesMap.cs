using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Common
{
    public partial class DeliveryRoutesMap: NopEntityTypeConfiguration<DeliveryRoutes>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<DeliveryRoutes> builder)
        {
            builder.ToTable(nameof(DeliveryRoutes));
            builder.HasKey(deliveryRoutes => deliveryRoutes.Id);

            builder.Property(deliveryRoutes => deliveryRoutes.Location).HasMaxLength(50);
            builder.Property(deliveryRoutes => deliveryRoutes.Name).HasMaxLength(50);
            builder.Property(deliveryRoutes => deliveryRoutes.Minimum);
            builder.Property(deliveryRoutes => deliveryRoutes.Available);



            base.Configure(builder);
        }

        #endregion
    }
}
