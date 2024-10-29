using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.NN;

namespace Nop.Data.Mapping.NN
{
    public class PendingDataToSyncMap : NopEntityTypeConfiguration<PendingDataToSync>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<PendingDataToSync> builder)
        {
            builder.ToTable(nameof(PendingDataToSync));
            builder.HasKey(c => c.Id);
        }

        #endregion
    }
}
