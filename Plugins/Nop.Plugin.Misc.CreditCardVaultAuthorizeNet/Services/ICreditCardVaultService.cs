using Nop.Core.Domain.Customers;
using Nop.Services.Payments;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services
{
    public interface ICreditCardVaultService
    {
        string SaveCreditCard(Customer customer, ProcessPaymentRequest processPaymentRequest);
    }
}
