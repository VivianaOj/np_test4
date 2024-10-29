using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Data.Mapping.Customers
{
    public partial class CustomerPaymentAuthorizeMap : NopEntityTypeConfiguration<CustomerPaymentProfile>
    {
        #region Methods

        /// <summary>
        /// Configures the entity
        /// </summary>
        /// <param name="builder">The builder to be used to configure the entity</param>
        public override void Configure(EntityTypeBuilder<CustomerPaymentProfile> builder)
        {
            builder.ToTable(nameof(CustomerPaymentProfile));
            builder.HasKey(customerPayment => customerPayment.Id);

            builder.Property(customerPayment => customerPayment.CustomerProfileId).HasMaxLength(1000);
            builder.Property(customerPayment => customerPayment.CustomerPaymentProfileList).HasMaxLength(1000);
            builder.Property(customerPayment => customerPayment.ResultCode).HasMaxLength(1000);
            builder.Property(customerPayment => customerPayment.CustomerId).HasMaxLength(400);


            base.Configure(builder);
        }

        #endregion
    }
}
