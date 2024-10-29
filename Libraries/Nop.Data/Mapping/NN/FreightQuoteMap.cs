using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.NN;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.NN
{
   
        public partial class FreightQuoteMap : NopEntityTypeConfiguration<FreightQuote>
        {
            #region Methods

            /// <summary>
            /// Configures the entity
            /// </summary>
            /// <param name="builder">The builder to be used to configure the entity</param>
            public override void Configure(EntityTypeBuilder<FreightQuote> builder)
            {
                builder.ToTable(nameof(FreightQuote));
                builder.HasKey(order => order.Id);

                builder.Property(order => order.RequestDate).HasColumnType("datetime2");
                builder.Property(order => order.Name).HasColumnType("nvarchar(-1)");
                builder.Property(order => order.Email).HasColumnType("nvarchar(-1)");
                builder.Property(order => order.Phone).HasColumnType("nvarchar(-1)");
                builder.Property(order => order.Infomation).HasColumnType("nvarchar(-1)");
                builder.Property(order => order.Items).HasColumnType("nvarchar(-1)");
                builder.Property(order => order.BillingAddress_Id).HasColumnType("int");
                builder.Property(order => order.Shippng_Address_Id).HasColumnType("int");


            base.Configure(builder);
            }

            #endregion
        }
    }