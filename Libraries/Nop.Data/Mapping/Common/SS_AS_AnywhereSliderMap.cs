using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Common
{
    public partial class SS_AS_AnywhereSliderMap: NopEntityTypeConfiguration<SS_AS_AnywhereSlider>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SS_AS_AnywhereSlider> builder)
        {
            builder.ToTable(nameof(SS_AS_AnywhereSlider));
            builder.HasKey(AnywhereSlider => AnywhereSlider.Id);

            builder.Property(AnywhereSlider => AnywhereSlider.SystemName).HasColumnName("SystemName");
            builder.Property(AnywhereSlider => AnywhereSlider.SliderType).HasColumnName("SliderType");
            builder.Property(AnywhereSlider => AnywhereSlider.LanguageId).HasColumnName("LanguageId");
            builder.Property(AnywhereSlider => AnywhereSlider.LimitedToStores).HasColumnName("LimitedToStores");


            base.Configure(builder);
        }

        #endregion
    }
}
