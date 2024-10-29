using Newtonsoft.Json;
using Nop.Core.Domain.Customers;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests;
using Nop.Services.Common;
using Nop.Services.Logging;
using System.IO;
using System.Net;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services
{
    public class CustomerAuthorizeNetService : ICustomerAuthorizeNetService
    {
        #region Fields
        private readonly IConnectionService _connectionService;
        private readonly ICustomerAuthorizeNet _customerAuthorizeNet;
        private readonly ILogger _logger;

        #endregion

        #region Ctor
        public CustomerAuthorizeNetService(IConnectionService connectionService, ILogger logger, ICustomerAuthorizeNet customerAuthorizeNet)
        {
            _connectionService = connectionService;
            _customerAuthorizeNet = customerAuthorizeNet;
            _logger = logger;

        }
        #endregion

        #region Methods

        public ResponseModel CreateCustomer(CreateCustomerProfileRequestModel model)
        {
            var createCustomer = _connectionService.GetConnection("createCustomerProfile");
            using (var streamWriter = new StreamWriter(createCustomer.GetRequestStream()))
            {

                string json = JsonConvert.SerializeObject(model);
                //_logger.Warning("json"+ json);
                streamWriter.Write(json);
            }
            
            var httpResponse = (HttpWebResponse)createCustomer.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                ResponseModel resp = new ResponseModel();
                resp = JsonConvert.DeserializeObject<ResponseModel>(result);
                return resp;               
            }            
        }

        public ResponseModel CreateCustomerPayment(CreateCustomerPaymentProfileRequestModel model)
        {
            var createPayment = _connectionService.GetConnection("createCustomerPaymentProfile");
            using (var streamWriter = new StreamWriter(createPayment.GetRequestStream()))
            {

                string json = JsonConvert.SerializeObject(model);
                //_logger.Warning("json" + json);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)createPayment.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                ResponseModel resp = new ResponseModel();
                resp = JsonConvert.DeserializeObject<ResponseModel>(result);
                return resp;               
            }
        }

        public ResponseGetCustomerPaymentProfileModel GetCustomerPaymentProfile(GetCustomerPaymentProfileRequestModel model)
        {
            var getCustomer = _connectionService.GetConnection("getCustomerPaymentProfile");
            using (var streamWriter = new StreamWriter(getCustomer.GetRequestStream()))
            {

                string json = JsonConvert.SerializeObject(model);
                //_logger.Warning("json" + json);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)getCustomer.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                Core.Domain.Customers.ResponseGetCustomerPaymentProfileModel resp = new Core.Domain.Customers.ResponseGetCustomerPaymentProfileModel();
                resp = JsonConvert.DeserializeObject<Core.Domain.Customers.ResponseGetCustomerPaymentProfileModel>(result);
                return resp;
            }
        }

        public ResponseModel DeleteCustomerPaymentProfile(DeleteCustomerPaymentProfileRequestModel model)
        {
            var deleteCustomer = _connectionService.GetConnection("deleteCustomerPaymentProfileRequest");
            var DeleteCustomerPaymentProfileRequestModel = new DeleteCustomerPaymentProfileRequestModel
            {
                customerPaymentProfileId= model.customerPaymentProfileId,
                customerProfileId=model.customerProfileId,
                merchantAuthentication=model.merchantAuthentication
            };
            
            using (var streamWriter = new StreamWriter(deleteCustomer.GetRequestStream()))
            {

                string json = JsonConvert.SerializeObject(DeleteCustomerPaymentProfileRequestModel);
                //_logger.Warning("json" + json);
                streamWriter.Write(json);
            }

            var httpResponse = (HttpWebResponse)deleteCustomer.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
                ResponseModel resp = new ResponseModel();
                resp = JsonConvert.DeserializeObject<ResponseModel>(result);
                return resp;

            }
        }

        private void InsertPaymentProfileNop(ResponseModel model) 
        {
            CustomerProfileAuthorize customerProfile = new CustomerProfileAuthorize();
            if (model.messages.resultCode == "Error")
            {
                customerProfile.CustomerProfileId = "Error";
                customerProfile.CustomerPaymentProfileList = "No Create";
                customerProfile.ResultCode = model.messages.resultCode;
                //Id pending
            }
            else {
                customerProfile.CustomerProfileId = model.customerProfileId;
                customerProfile.CustomerPaymentProfileList = model.CustomerPaymentProfileIdList[0];
                customerProfile.ResultCode = model.messages.resultCode;
                //Id pending
            }

            _customerAuthorizeNet.InsertCustomerProfile(customerProfile);
        }

        private void InsertCustomerPaymentNop(ResponseModel model)
        {
            CustomerPaymentProfile paymentProfile = new CustomerPaymentProfile();
            if(model.messages.resultCode == "Error"){
                paymentProfile.CustomerProfileId = "Error";
                paymentProfile.CustomerPaymentProfileList = "No Create";
                paymentProfile.ResultCode = model.messages.resultCode;
                //Id pending
            }
            else
            {
                paymentProfile.CustomerProfileId = model.customerProfileId;
                paymentProfile.CustomerPaymentProfileList = model.messages.message[0].text;
                paymentProfile.ResultCode = model.messages.resultCode;
                //Id pending
            }

            _customerAuthorizeNet.InsertPaymentProfile(paymentProfile);
        }

    }

    #endregion
}
