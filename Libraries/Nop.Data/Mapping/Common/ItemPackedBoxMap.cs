using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Common
{
    public partial class ItemPackedBoxMap : NopEntityTypeConfiguration<BSItemPackedBox>
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
