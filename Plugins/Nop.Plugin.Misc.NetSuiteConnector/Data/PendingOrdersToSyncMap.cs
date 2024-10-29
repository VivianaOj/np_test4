﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;

namespace Nop.Plugin.Misc.NetSuiteConnector.Data
{
    public class PendingOrdersToSyncMap : NopEntityTypeConfiguration<PendingOrdersToSync>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PendingOrdersToSync> builder)
        {
            builder.ToTable(nameof(PendingOrdersToSync));
            builder.HasKey(c => c.Id);
        }

        #endregion
    }
}
