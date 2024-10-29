using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Data
{
        public class EntityStatusMap : NopEntityTypeConfiguration<EntityStatus>
        {
            #region Methods

            /// <summary>
            /// Configures the entity
            /// </summary>
            /// <param name="builder">The builder to be used to configure the entity</param>
            public override void Configure(EntityTypeBuilder<EntityStatus> builder)
            {
                builder.ToTable(nameof(EntityStatus));
                builder.HasKey(c => c.Id);
            }

            #endregion
        }
    }
