using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Common
{
    public partial class SS_AS_SliderImageMap: NopEntityTypeConfiguration<SS_AS_SliderImage>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<SS_AS_SliderImage> builder)
        {
            builder.ToTable(nameof(SS_AS_SliderImage));
            builder.HasKey(SliderImage => SliderImage.Id);

            builder.Property(SliderImage => SliderImage.DisplayText).HasColumnName("DisplayText");
            builder.Property(SliderImage => SliderImage.Url).HasColumnName("Url");
            builder.Property(SliderImage => SliderImage.Alt).HasColumnName("Alt");
            builder.Property(SliderImage => SliderImage.Visible).HasColumnName("Visible");
            builder.Property(SliderImage => SliderImage.DisplayOrder).HasColumnName("DisplayOrder");
            builder.Property(SliderImage => SliderImage.PictureId).HasColumnName("PictureId");
            builder.Property(SliderImage => SliderImage.MobilePictureId).HasColumnName("MobilePictureId");
            builder.Property(SliderImage => SliderImage.SliderId).HasColumnName("SliderId");


            base.Configure(builder);
        }

        #endregion
    }
}
