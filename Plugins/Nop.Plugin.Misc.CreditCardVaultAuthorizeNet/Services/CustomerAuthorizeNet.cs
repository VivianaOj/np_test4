using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services
{
    public partial class CustomerAuthorizeNet : ICustomerAuthorizeNet
    {
        #region Fields
        private readonly IRepository<CustomerProfileAuthorize> _customerProfileRepository;
        private readonly IRepository<CustomerPaymentProfile> _customerPaymentRepository;
        private readonly IRepository<CustomerAddressMapping> _customerAddress;
        #endregion

        #region Ctor

        public CustomerAuthorizeNet(IRepository<CustomerProfileAuthorize> customerProfileRepository, IRepository<CustomerPaymentProfile> customerPaymentRepository, IRepository<CustomerAddressMapping> customerAddress)
        {
            _customerProfileRepository = customerProfileRepository;
            _customerPaymentRepository = customerPaymentRepository;
            _customerAddress = customerAddress;
        }
        
        #endregion

        #region Methods

        public void InsertCustomerProfile(CustomerProfileAuthorize CustomerProfile)
        {
            if (CustomerProfile == null)
                throw new ArgumentNullException(nameof(CustomerProfile));

            _customerProfileRepository.Insert(CustomerProfile);
        }

        public void InsertPaymentProfile(CustomerPaymentProfile PaymentProfile)
        {
            if (PaymentProfile == null)
                throw new ArgumentNullException(nameof(PaymentProfile));

            _customerPaymentRepository.Insert(PaymentProfile);
        }

        public CustomerPaymentProfile GetProfileByCustomerId(int CustomerId)
        {
            if (CustomerId == 0)
                throw new ArgumentNullException(nameof(CustomerId));

            CustomerPaymentProfile result = new CustomerPaymentProfile();
            var query = _customerProfileRepository.Table.Where(x => x.CustomerId == CustomerId && x.CustomerProfileId != "Error").FirstOrDefault();

            if (query != null)
            {
                result.CustomerId = query.CustomerId;
                result.CustomerPaymentProfileList = query.CustomerPaymentProfileList;
                result.CustomerProfileId = query.CustomerProfileId;
                result.ResultCode = query.ResultCode;

            }

            return result;
        }

        public int searchAddresId(int CustomerId)
        {
            int result = 0;
            if (CustomerId == 0)
                throw new ArgumentNullException(nameof(CustomerId));
            var query = _customerAddress.Table.Where(x => x.CustomerId == CustomerId).FirstOrDefault();
            if (query!=null)
            {
              result = query.AddressId;
            }
            return result;
        }

        public bool SearchProfileByCustomerId(int CustomerId)
        {
            if (CustomerId == 0)
                throw new ArgumentNullException(nameof(CustomerId));
            bool flag = false;
            List<CustomerProfileAuthorize> list = new List<CustomerProfileAuthorize>();
            var query = _customerProfileRepository.Table.Where(x => x.CustomerId == CustomerId && x.CustomerProfileId != "Error");
            foreach (var item in query)
            {
                list.Add(new CustomerProfileAuthorize
                {
                    CustomerId = item.CustomerId,
                    CustomerProfileId = item.CustomerProfileId,
                    CustomerPaymentProfileList = item.CustomerPaymentProfileList,
                    ResultCode = item.ResultCode,
                    Id = item.Id
                });
            }

            if (list.Count > 0)
            {
                flag = true;
            }
            else
            {
                flag = false;
            }

            return flag;

        }

        #endregion
    }
}