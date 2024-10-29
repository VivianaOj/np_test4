using Nop.Core.Domain.Customers;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Services.Customers
{
    public interface ICustomerAuthorizeNetService
    {
        ResponseModel CreateCustomer(CreateCustomerProfileRequestInfoModel model);
        ResponseModel CreateCustomerPayment(CreateCustomerPaymentProfileRequestInfoModel model);
        CustomerPaymentProfile GetProfileByCustomerId(int CustomerId);
        CustomerPaymentProfile GetPaymentProfile(string ProfileId, int id);
        CustomerPaymentProfile GetProfileByProfileId(string ProfileId, int id);
        ResponseGetCustomerPaymentProfileModel GetCustomerPaymentProfile(GetCustomerPaymentProfileRequestModel model);
        bool SearchProfileByCustomerId(int CustomerId);
        ResponseModel DeleteCustomerPaymentProfile(DeleteCustomerPaymentProfileRequestModel model);
        void InsertCustomerProfile(CustomerProfileAuthorize CustomerProfile);
        void InsertPaymentProfileNop(ResponseModel model);
        void InsertPaymentProfile(CustomerPaymentProfile PaymentProfile);

        void DeletePaymentById(int Id, int CustomerId);

        CustomerPaymentProfile GetPaymentProfileByProfile(string ProfileId, string paymentId, int CustomerId);

        List<ResponseGetCustomerPaymentProfileModel> GetCustomerProfileIdsRequest(string Email, int CustomerId);
    }
}
