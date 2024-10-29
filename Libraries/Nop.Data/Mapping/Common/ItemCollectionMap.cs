
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;

namespace Nop.Data.Mapping.Common
{
    public class ItemCollectionMap : NopEntityTypeConfiguration<ItemCollection>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<ItemCollection> builder)
        {
            builder.ToTable(nameof(ItemCollection));
            builder.HasKey(itemCollection => itemCollection.Id);

            base.Configure(builder);
        }

        #endregion
    }
}
