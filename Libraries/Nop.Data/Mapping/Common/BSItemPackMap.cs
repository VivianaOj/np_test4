using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Common
{
    public partial class BSItemPackMap : NopEntityTypeConfiguration<BSItemPack>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<BSItemPack> builder)
        {
            builder.ToTable(nameof(BSItemPack));
            builder.HasKey(c => c.Id);
            
        }

        #endregion
    }
}