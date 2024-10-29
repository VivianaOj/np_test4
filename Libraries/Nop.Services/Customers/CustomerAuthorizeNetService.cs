using Newtonsoft.Json;
using Nop.Core.Data;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace Nop.Services.Customers
{
    public class CustomerAuthorizeNetService : ICustomerAuthorizeNetService
    {
        #region Fields
        private readonly IConnectionService _connectionService;
        // private readonly ICustomerAuthorizeNet _customerAuthorizeNet;
        private readonly IRepository<CustomerProfileAuthorize> _customerProfileRepository;
        private readonly ISettingService _settingService;
        private readonly IRepository<CustomerPaymentProfile> _customerPaymentRepository;
        private readonly ILogger _logger;
        #endregion


        #region Ctor
        public CustomerAuthorizeNetService(IConnectionService connectionService, ILogger logger,
            IRepository<CustomerProfileAuthorize> customerProfileRepository, ISettingService settingService, IRepository<CustomerPaymentProfile> customerPaymentRepository
           //, ICustomerAuthorizeNet customerAuthorizeNet
           )
        {
            _logger = logger;
            _connectionService = connectionService;
            _customerProfileRepository = customerProfileRepository;
            // _customerAuthorizeNet = customerAuthorizeNet;
            _settingService = settingService;
            _customerPaymentRepository = customerPaymentRepository;

        }

        public bool SearchProfileByCustomerId(int CustomerId)
        {
            if (CustomerId == 0)
                throw new ArgumentNullException(nameof(CustomerId));
            bool flag = false;
            List<CustomerProfileAuthorize> list = new List<CustomerProfileAuthorize>();
            var query = _customerProfileRepository.Table.Where(x => x.CustomerId == CustomerId && x.CustomerProfileId!="Error");
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

        public void InsertCustomerProfile(CustomerProfileAuthorize CustomerProfile)
        {
            if (CustomerProfile == null)
                throw new ArgumentNullException(nameof(CustomerProfile));

            _customerProfileRepository.Insert(CustomerProfile);
        }


        #endregion


        #region Methods
        public ResponseModel CreateCustomer(CreateCustomerProfileRequestInfoModel model)
        {
            var createCustomer = _connectionService.GetConnection("createCustomerProfile");
            using (var streamWriter = new StreamWriter(createCustomer.GetRequestStream()))
            {

                string json = JsonConvert.SerializeObject(model);

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

        public ResponseModel CreateCustomerPayment(CreateCustomerPaymentProfileRequestInfoModel model)
        {
            var createPayment = _connectionService.GetConnection("createCustomerPaymentProfile");
            using (var streamWriter = new StreamWriter(createPayment.GetRequestStream()))
            {

                string json = JsonConvert.SerializeObject(model);

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
            try
            {
                var getCustomer = _connectionService.GetConnection("getCustomerPaymentProfile");
                model.getCustomerProfileRequest.unmaskExpirationDate = true;
                using (var streamWriter = new StreamWriter(getCustomer.GetRequestStream()))
                {

                    string json = JsonConvert.SerializeObject(model);
                    streamWriter.Write(json);
                }

                var httpResponse = (HttpWebResponse)getCustomer.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    ResponseGetCustomerPaymentProfileModel resp = new ResponseGetCustomerPaymentProfileModel();
                    resp = JsonConvert.DeserializeObject<ResponseGetCustomerPaymentProfileModel>(result);

                    //if (resp.profile?.paymentProfiles != null)
                    //{
                    //    resp.profile.paymentProfiles = resp.profile.paymentProfiles
                    //        .GroupBy(x => new { x.payment.creditCard.cardType, x.payment.creditCard.cardNumber }).Select(y => y.First()).ToList();
                    //}
                    return resp;

                }
            }
            catch (Exception exc)
            {

            }
            return new ResponseGetCustomerPaymentProfileModel();
        }

        public List<ResponseGetCustomerPaymentProfileModel> GetCustomerProfileIdsRequest(string Email, int CustomerId)
        {
            try
            {
                string TransactionKey = _settingService.GetSetting("authorizenetpaymentsettings.transactionkey").Value;
                string LoginId = _settingService.GetSetting("authorizenetpaymentsettings.loginid").Value;
                var getCustomer = _connectionService.GetConnection("getCustomerProfile");

                merchantAuthenticationModelRequest model = new merchantAuthenticationModelRequest();
                model.merchantAuthentication = new MerchantAuthenticationModel
                {
                    transactionKey = TransactionKey,
                    name = LoginId
                };

                var modelRequest = new merchantAuthenticationModelRequest
                {
                    merchantAuthentication = model.merchantAuthentication
                };
                //var httpResponse = (HttpWebResponse)getCustomer.GetResponse();
                using (var streamWriter = new StreamWriter(getCustomer.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(model);
                    streamWriter.Write(json);
                }
                var httpResponse = (HttpWebResponse)getCustomer.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    ResponseGetCustomerPaymentProfileModel resp = new ResponseGetCustomerPaymentProfileModel();
                    resp = JsonConvert.DeserializeObject<ResponseGetCustomerPaymentProfileModel>(result);
                    //return resp;

                }



            }
            catch (Exception exception)
            {
                _logger.Error(exception.Message, exception);
            }
            return null;
        }

        public CustomerPaymentProfile GetProfileByCustomerId(int CustomerId)
        {
            if (CustomerId == 0)
                throw new ArgumentNullException(nameof(CustomerId));

            CustomerPaymentProfile result = new CustomerPaymentProfile();
            var query = _customerPaymentRepository.Table.Where(x => x.CustomerId == CustomerId && x.ResultCode!="Error").FirstOrDefault();
            //if (query != null)
            //{
            //    result.CustomerId = query.CustomerId;
            //    result.CustomerPaymentProfileList = query.CustomerPaymentProfileList;
            //    result.CustomerProfileId = query.CustomerProfileId;
            //    result.ResultCode = query.ResultCode;
            //}


            return query;
        }

        public void DeletePaymentById(int cp, int ccp)
        {
            if (ccp == 0)
                throw new ArgumentNullException(nameof(ccp));

            CustomerPaymentProfile result = new CustomerPaymentProfile();

            var query = _customerPaymentRepository.Table.Where(x => x.CustomerPaymentProfileList == ccp.ToString()).FirstOrDefault();

            _customerPaymentRepository.Delete(query);

        }

        public ResponseModel DeleteCustomerPaymentProfile(DeleteCustomerPaymentProfileRequestModel model)
        {
            if (model != null)
            {
                string TransactionKey = _settingService.GetSetting("authorizenetpaymentsettings.transactionkey").Value;
                string LoginId = _settingService.GetSetting("authorizenetpaymentsettings.loginid").Value;

                model.merchantAuthentication = new MerchantAuthenticationModel
                {
                    transactionKey = TransactionKey,
                    name = LoginId
                };
                var modelDelete = new deleteCustomerPaymentProfile();

                var DeleteCustomerPaymentProfileRequestModel = new DeleteCustomerPaymentProfileRequestModel
                {
                    customerPaymentProfileId = model.customerPaymentProfileId,
                    customerProfileId = model.customerProfileId,
                    merchantAuthentication = model.merchantAuthentication
                };
                modelDelete.deleteCustomerPaymentProfileRequest = DeleteCustomerPaymentProfileRequestModel;


                var deleteCustomer = _connectionService.GetConnection("deleteCustomerPaymentProfileRequest");
                using (var streamWriter = new StreamWriter(deleteCustomer.GetRequestStream()))
                {
                    string json = JsonConvert.SerializeObject(modelDelete);
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
            return null;
        }

        public void InsertPaymentProfileNop(ResponseModel model)
        {
            CustomerProfileAuthorize customerProfile = new CustomerProfileAuthorize();
            if (model.messages.resultCode == "Error")
            {
                customerProfile.CustomerProfileId = "Error";
                customerProfile.CustomerPaymentProfileList = "No Create";
                customerProfile.ResultCode = model.messages.resultCode;
                //Id pending
            }
            else
            {
                customerProfile.CustomerProfileId = model.customerProfileId;
                customerProfile.CustomerPaymentProfileList = model.CustomerPaymentProfileIdList[0];
                customerProfile.ResultCode = model.messages.resultCode;
                //Id pending
            }



            //    _customerAuthorizeNet.InsertCustomerProfile(customerProfile);
            //}

            //private void InsertCustomerPaymentNop(ResponseModel model)
            //{
            //    CustomerPaymentProfile paymentProfile = new CustomerPaymentProfile();
            //    if(model.messages.resultCode == "Error"){
            //        paymentProfile.CustomerProfileId = "Error";
            //        paymentProfile.CustomerPaymentProfileList = "No Create";
            //        paymentProfile.ResultCode = model.messages.resultCode;
            //        //Id pending

            //    }
            //    else
            //    {
            //        paymentProfile.CustomerProfileId = model.customerProfileId;
            //        paymentProfile.CustomerPaymentProfileList = model.messages.message[0].text;
            //        paymentProfile.ResultCode = model.messages.resultCode;
            //        //Id pending

            //    }

            //    _customerAuthorizeNet.InsertPaymentProfile(paymentProfile);
            //}

        }

        public void InsertPaymentProfile(CustomerPaymentProfile PaymentProfile)
        {
            if (PaymentProfile == null)
                throw new ArgumentNullException(nameof(PaymentProfile));

            _customerPaymentRepository.Insert(PaymentProfile);
        }

        public CustomerPaymentProfile GetProfileByProfileId(string ProfileId, int id)
        {
            if (ProfileId == null)
                throw new ArgumentNullException(nameof(ProfileId));

            CustomerPaymentProfile result = new CustomerPaymentProfile();
            var query = _customerProfileRepository.Table.Where(x => x.CustomerId == id && x.CustomerPaymentProfileList == ProfileId && x.ResultCode != "Error").FirstOrDefault();
            if (query != null)
            {
                result.CustomerId = query.CustomerId;
                result.CustomerPaymentProfileList = query.CustomerPaymentProfileList;
                result.CustomerProfileId = query.CustomerProfileId;
                result.ResultCode = query.ResultCode;
            }


            return result;
        }

        public CustomerPaymentProfile GetPaymentProfile(string ProfileId, int id)
        {
            if (ProfileId == null)
                throw new ArgumentNullException(nameof(ProfileId));

            CustomerPaymentProfile result = new CustomerPaymentProfile();
            var query = _customerPaymentRepository.Table.Where(x => x.CustomerId == id && x.CustomerPaymentProfileList==ProfileId && x.ResultCode != "Error").FirstOrDefault();
            if (query != null)
            {
                result.CustomerId = query.CustomerId;
                result.CustomerPaymentProfileList = query.CustomerPaymentProfileList;
                result.CustomerProfileId = query.CustomerProfileId;
                result.ResultCode = query.ResultCode;
            }


            return result;
        }

        public CustomerPaymentProfile GetPaymentProfileByProfile(string ProfileId, string paymentId, int CustomerId)
        {
            CustomerPaymentProfile result = new CustomerPaymentProfile();
            if (ProfileId != null)
            {
                result = _customerPaymentRepository.Table.Where(x => x.CustomerProfileId == ProfileId && x.CustomerPaymentProfileList == paymentId && x.CustomerId == CustomerId && x.ResultCode != "Error").FirstOrDefault();

            }
            return result;
        }

    }

        public class CreateCustomerProfileRequestInfoModel
        {
            public CreateCustomerProfileRequestInfoModel()
            {
                createCustomerProfileRequest = new CreateCustomerProfileInfoRequest();
            }

            public CreateCustomerProfileInfoRequest createCustomerProfileRequest { get; set; }
        }

        public class CreateCustomerProfileInfoRequest
        {
            public CreateCustomerProfileInfoRequest()
            {
                merchantAuthentication = new MerchantAuthenticationModel();
                profile = new ProfileInfoCustomerModel();
            }

            public MerchantAuthenticationModel merchantAuthentication { get; set; }
            public ProfileInfoCustomerModel profile { get; set; }
            public string validationMode { get; set; }
        }

        public class ProfileInfoCustomerModel
        {
            public ProfileInfoCustomerModel()
            {
                paymentProfiles = new PaymentProfilesInfoModel();
            }

            public string merchantCustomerId { get; set; }
            public string description { get; set; }
            public string email { get; set; }
            public PaymentProfilesInfoModel paymentProfiles { get; set; }
        }
        public class PaymentProfilesInfoModel
        {
            public PaymentProfilesInfoModel()
            {
                payment = new PaymentModelInfo();
            }

            public string customerType { get; set; }
            public PaymentModelInfo payment { get; set; }
            //public bool DefaultPaymentProfile { get; set; }
        }

        public class CreateCustomerPaymentProfileRequestInfoModel
        {
            public CreateCustomerPaymentProfileRequestInfoModel()
            {
                createCustomerPaymentProfileRequest = new CreateCustomerPaymentProfileInfoRequest();
            }

            public CreateCustomerPaymentProfileInfoRequest createCustomerPaymentProfileRequest { get; set; }
        }

        public class CreateCustomerPaymentProfileInfoRequest
        {
            public CreateCustomerPaymentProfileInfoRequest()
            {
                merchantAuthentication = new MerchantAuthenticationModel();
                paymentProfile = new PaymentProfileInfo();
            }

            public MerchantAuthenticationModel merchantAuthentication { get; set; }
            public string customerProfileId { get; set; }
            public PaymentProfileInfo paymentProfile { get; set; }
            public string validationMode { get; set; }
        }

        public class PaymentProfileInfo
        {
            public PaymentProfileInfo()
            {
                billTo = new BillToModel();
                payment = new PaymentModelInfo();
            }

            public BillToModel billTo { get; set; }
            public PaymentModelInfo payment { get; set; }
            public bool defaultPaymentProfile { get; set; }
        }

        public class PaymentModelInfo
        {
            public PaymentModelInfo()
            {
                creditCard = new CreditCardModelInfo();
            }

            public CreditCardModelInfo creditCard { get; set; }
        }

        public class CreditCardModelInfo
        {
            public string cardNumber { get; set; }
            public string expirationDate { get; set; }
        }
    
    #endregion
}
