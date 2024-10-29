using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;

namespace Nop.Data.Mapping.Common
{
    public partial class ZipCodeMap: NopEntityTypeConfiguration<Zipcode>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<Zipcode> builder)
        {
            builder.ToTable(nameof(Zipcode));
            builder.HasKey(attribute => attribute.Id);

            builder.Property(attribute => attribute.Code).HasMaxLength(400).IsRequired();

            builder.HasOne(address => address.Country)
                .WithMany()
                .HasForeignKey(address => address.CountryId);

            builder.HasOne(address => address.StateProvince)
                .WithMany()
                .HasForeignKey(address => address.StateProvinceId);

            base.Configure(builder);
        }

        #endregion
    }
}
