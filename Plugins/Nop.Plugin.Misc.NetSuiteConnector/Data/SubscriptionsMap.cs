using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Data.Mapping;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;

namespace Nop.Plugin.Misc.NetSuiteConnector.Data
{
    public class SubscriptionsMap : NopEntityTypeConfiguration<Subscriptions>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Subscriptions> builder)
        {
            builder.ToTable(nameof(Subscriptions));
            builder.HasKey(i => i.Id);
        }

        #endregion
    }
}
