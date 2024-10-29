using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.NN;

namespace Nop.Data.Mapping.NN
{
    public partial class LogNetsuiteImportMap : NopEntityTypeConfiguration<LogNetsuiteImport>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<LogNetsuiteImport> builder)
        {
            builder.ToTable(nameof(LogNetsuiteImport));
            builder.HasKey(x => x.Id);

            builder.Property(x =>x.DateCreate).HasColumnType("datetime2");
           
            base.Configure(builder);
        }

        #endregion
    }
}
