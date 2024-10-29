using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Customers
{
    public partial class CustomerProfileAuthorizeMap : NopEntityTypeConfiguration<CustomerProfileAuthorize>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<CustomerProfileAuthorize> builder)
        {
            builder.ToTable(nameof(CustomerProfileAuthorize));
            builder.HasKey(customerProfile => customerProfile.Id);

            builder.Property(customerProfile => customerProfile.CustomerProfileId).HasMaxLength(1000);
            builder.Property(customerProfile => customerProfile.CustomerPaymentProfileList).HasMaxLength(1000);
            builder.Property(customerProfile => customerProfile.ResultCode).HasMaxLength(1000);
            builder.Property(customerProfile => customerProfile.CustomerId).HasMaxLength(400);


            base.Configure(builder);
        }

        #endregion
    }

}
