using Nop.Core.Domain.Customers;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services
{
    public interface ICustomerAuthorizeNet
    {
        void InsertCustomerProfile(CustomerProfileAuthorize CustomerProfile);
        void InsertPaymentProfile(CustomerPaymentProfile PaymentProfile);
        bool SearchProfileByCustomerId(int CustomerId);
        CustomerPaymentProfile GetProfileByCustomerId(int CustomerId);
        int searchAddresId(int CustomerId);

    }
}
