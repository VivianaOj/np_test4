using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Responses;
using ResponseGetCustomerPaymentProfileModel = Nop.Core.Domain.Customers.ResponseGetCustomerPaymentProfileModel;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services
{
    public interface ICustomerAuthorizeNetService
    {
        ResponseModel CreateCustomer(CreateCustomerProfileRequestModel model);
        ResponseModel CreateCustomerPayment(CreateCustomerPaymentProfileRequestModel model);
        ResponseGetCustomerPaymentProfileModel GetCustomerPaymentProfile(GetCustomerPaymentProfileRequestModel model);
        ResponseModel DeleteCustomerPaymentProfile(DeleteCustomerPaymentProfileRequestModel model);
    }
}
