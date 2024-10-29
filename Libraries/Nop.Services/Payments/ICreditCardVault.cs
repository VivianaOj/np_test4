using Nop.Core.Domain.Customers;
using Nop.Services.Plugins;

namespace Nop.Services.Payments
{
    public partial interface ICreditCardVault : IPlugin
    {
        string SaveCreditCard(Customer customer, ProcessPaymentRequest processPaymentRequest);
    }
}
