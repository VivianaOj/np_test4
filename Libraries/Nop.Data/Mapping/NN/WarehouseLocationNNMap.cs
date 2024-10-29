using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.NN;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.NN
{
   
        public partial class WarehouseLocationNNMap : NopEntityTypeConfiguration<WarehouseLocationNN>
        {
            #region Methods

            /// <summary>
            /// Configures the entity
            /// </summary>
            /// <param name="builder">The builder to be used to configure the entity</param>
            public override void Configure(EntityTypeBuilder<WarehouseLocationNN> builder)
            {
                builder.ToTable(nameof(WarehouseLocationNN));
                builder.HasKey(c => c.Id);

            base.Configure(builder);
            }

            #endregion
        }
    }