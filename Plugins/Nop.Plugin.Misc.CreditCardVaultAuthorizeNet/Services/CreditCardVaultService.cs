using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Requests;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Models.Responses;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Logging;
using Nop.Services.Payments;
using System;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services
{
    public class CreditCardVaultService : ICreditCardVaultService
    {
        #region Fields

        private readonly IAddressService _addressService;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly ICustomerAuthorizeNet _customerAuthorizeNet;
        private readonly ICustomerAuthorizeNetService _customerAuthorizeNetService;

        #endregion

        #region Ctor

        public CreditCardVaultService(IAddressService addressService,
        ISettingService settingService,
        ILogger logger,
        ICustomerAuthorizeNet customerAuthorizeNet,
        ICustomerAuthorizeNetService customerAuthorizeNetService)
        {
            _addressService = addressService;
            _settingService = settingService;
            _logger = logger;
            _customerAuthorizeNet = customerAuthorizeNet;
            _customerAuthorizeNetService = customerAuthorizeNetService;
        }

        #endregion

        #region Methods

        public string SaveCreditCard(Customer customer, ProcessPaymentRequest processPaymentRequest)
        {
            var messageStatus = "";

            try
            {
                var creditCardVaultAuthorizeNetSettings = _settingService.LoadSetting<CreditCardVaultAuthorizeNetSettings>();

                if (creditCardVaultAuthorizeNetSettings.Enabled)
                {
                    var currentCustomer = customer;
                    var TransactionKey = creditCardVaultAuthorizeNetSettings.TransactionKey;
                    var LoginId = creditCardVaultAuthorizeNetSettings.LoginId;

                    //Maping create Customer Profile
                    //Instance
                    CreateCustomerProfileRequestModel customerProfile = new CreateCustomerProfileRequestModel();
                    CreateCustomerProfileRequest customProfile = new CreateCustomerProfileRequest();
                    MerchantAuthenticationModel ma = new MerchantAuthenticationModel();
                    ProfileModel pm = new ProfileModel();
                    PaymentProfilesModel ppm = new PaymentProfilesModel();
                    PaymentModel payM = new PaymentModel();
                    CreditCardModel ccm = new CreditCardModel();

                    Address address = new Address();
                    if (processPaymentRequest.NewAddress?.Id == 0)
                    {
                        if (processPaymentRequest.NewAddress != null)
                        {
                            processPaymentRequest.NewAddress.Email = customer.Email;
                            processPaymentRequest.NewAddress.CreatedOnUtc = DateTime.Now;
                            processPaymentRequest.NewAddress.Active = true;
                            processPaymentRequest.NewAddress.FirstName = processPaymentRequest.CreditCardName;

                            _addressService.InsertAddress(processPaymentRequest.NewAddress);

                            if (processPaymentRequest.NewAddress.Id != 0)
                                address = _addressService.GetAddressById(processPaymentRequest.NewAddress.Id);
                        }
                        else
                        {
                            int AddressId = _customerAuthorizeNet.searchAddresId(customer.Id);
                            address = _addressService.GetAddressById(AddressId);
                        }

                    }
                    else
                    {
                        int AddressId = _customerAuthorizeNet.searchAddresId(customer.Id);
                        address = _addressService.GetAddressById(AddressId);
                    }

                    //Mapping and Add
                    
                    ResponseModel response = new ResponseModel();

                    bool exist = _customerAuthorizeNet.SearchProfileByCustomerId(currentCustomer.Id);
                    if (!exist)
                    {
                        //Load Models
                        customerProfile.createCustomerProfileRequest = customProfile;
                        customerProfile.createCustomerProfileRequest.merchantAuthentication = ma;
                        customerProfile.createCustomerProfileRequest.profile = pm;
                        customerProfile.createCustomerProfileRequest.profile.paymentProfiles = ppm;
                        customerProfile.createCustomerProfileRequest.profile.paymentProfiles.payment = payM;
                        customerProfile.createCustomerProfileRequest.profile.paymentProfiles.payment.creditCard = ccm;


                        //Mapping
                        customerProfile.createCustomerProfileRequest.merchantAuthentication.name = LoginId;
                        customerProfile.createCustomerProfileRequest.merchantAuthentication.transactionKey = TransactionKey;
                        customerProfile.createCustomerProfileRequest.profile.merchantCustomerId = currentCustomer.Id.ToString();
                        customerProfile.createCustomerProfileRequest.profile.description = "Profile description";
                        customerProfile.createCustomerProfileRequest.profile.email = currentCustomer.Email;

                    if (address != null)
                    {
                        var firstName = address.FirstName;
                        var lastName = address.LastName;
                        var email = address.Email;
                        var Address1 = address.Address1;
                        var city = address.City;
                        var state = address.StateProvince.Abbreviation.ToString();
                        var country = address.Country?.TwoLetterIsoCode;

                        var PhoneNumber = address.PhoneNumber;
                        var ZipPostalCode = address.ZipPostalCode;

                            if (address.FirstName?.Length > 50)
                                firstName = address.FirstName.Substring(0, 50);
                            if (address.LastName?.Length > 50)
                                lastName = address.LastName.Substring(0, 50);
                            if (address.Email?.Length > 20)
                                email = address.Email.Substring(0, 20);
                            if (address.Address1?.Length > 50)
                                Address1 = address.Address1.Substring(0, 50);

                            if (address.StateProvinceId.ToString().Length > 40)
                                Address1 = address.StateProvinceId.ToString().Substring(0, 40);


                            if (address.Country?.TwoLetterIsoCode.Length > 40)
                                country = address.Country.TwoLetterIsoCode.Substring(0, 40);

                            if (address.City?.Length > 40)
                                city = address.City.Substring(0, 40);
                            if (address.ZipPostalCode?.Length > 20)
                                ZipPostalCode = address.ZipPostalCode.Substring(0, 20);

                            if (address.PhoneNumber?.Length > 20)
                                PhoneNumber = address.PhoneNumber.Substring(0, 20);

                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.firstName = firstName;
                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.lastName = lastName;
                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.address = Address1;
                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.city = city;
                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.state = state;
                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.zip = ZipPostalCode;
                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.country = country;
                            customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.phoneNumber = PhoneNumber;

                            //customerProfile.createCustomerProfileRequest.profile.paymentProfiles.billTo.Company = address.Company;
                        }
                        customerProfile.createCustomerProfileRequest.profile.paymentProfiles.customerType = creditCardVaultAuthorizeNetSettings.CustomerType;
                        customerProfile.createCustomerProfileRequest.profile.paymentProfiles.payment.creditCard.cardNumber = processPaymentRequest.CreditCardNumber;
                        customerProfile.createCustomerProfileRequest.profile.paymentProfiles.payment.creditCard.expirationDate = processPaymentRequest.CreditCardExpireYear.ToString() + "-" + processPaymentRequest.CreditCardExpireMonth.ToString();
                        customerProfile.createCustomerProfileRequest.validationMode = creditCardVaultAuthorizeNetSettings.ValidationMode;



                        //_logger.Warning("exist id" + currentCustomer.Id);

                        response = _customerAuthorizeNetService.CreateCustomer(customerProfile);
                        CustomerProfileAuthorize newProfile = new CustomerProfileAuthorize();
                        CustomerPaymentProfile newPayment = new CustomerPaymentProfile();
                        if (response.messages.resultCode == "Error")
                        {
                            newProfile.CustomerProfileId = "Error";
                            newProfile.CustomerPaymentProfileList = response.messages.message[0].text;
                            newProfile.ResultCode = response.messages.resultCode;
                            newProfile.CustomerId = currentCustomer.Id;
                            
                            messageStatus = response.messages?.message[0]?.text;

                            if (response.messages.message[0].code == "E00039")
                            {
                                messageStatus = "This credit card already exists in Authorized.Net, please contact with your administrador.";
                            }
                           
                        }
                        else
                        {
                            newProfile.CustomerProfileId = response.customerProfileId;
                            if (response.CustomerPaymentProfileIdList != null)
                                newProfile.CustomerPaymentProfileList = response.CustomerPaymentProfileIdList[0];

                            newProfile.ResultCode = response.messages.resultCode;
                            newProfile.CustomerId = currentCustomer.Id;
                            _customerAuthorizeNet.InsertCustomerProfile(newProfile);

                            if (processPaymentRequest.CreditCardNumber != null)
                            {
                                newPayment.CustomerProfileId = response.customerProfileId;
                                if (response.CustomerPaymentProfileIdList != null)
                                    newPayment.CustomerPaymentProfileList = response.CustomerPaymentProfileIdList[0];

                                newPayment.ResultCode = response.messages.resultCode;
                                newPayment.CustomerId = currentCustomer.Id;
                                newPayment.BillingAddressId = address.Id;
                                newPayment.CustomerProfileId = response.customerProfileId;

                                _customerAuthorizeNet.InsertPaymentProfile(newPayment);
                            }
                            messageStatus = response.messages?.message[0]?.text;

                        }
                        
                        return messageStatus;

                    }
                    else
                    {
                        ////Mapping create Payment Profile
                        //CustomerPaymentProfile currentProfile = new CustomerPaymentProfile();
                        CustomerPaymentProfile currentProfile = _customerAuthorizeNet.GetProfileByCustomerId(customer.Id);


                        _logger.Warning("currentProfile" + currentProfile.CustomerProfileId);
                        //Instance
                        CreateCustomerPaymentProfileRequestModel paymentProfiel = new CreateCustomerPaymentProfileRequestModel();
                        CreateCustomerPaymentProfileRequest paymentProf = new CreateCustomerPaymentProfileRequest();
                        MerchantAuthenticationModel mam = new MerchantAuthenticationModel();
                        PaymentProfilesModel paymentpm = new PaymentProfilesModel();
                        PaymentModel payModel = new PaymentModel();
                        PaymentProfile payPro = new PaymentProfile();
                        BillToModel btm = new BillToModel();
                        PaymentModel payprofileModel = new PaymentModel();
                        CreditCardModel creditModel = new CreditCardModel();

                        //Load
                        paymentProfiel.createCustomerPaymentProfileRequest = paymentProf;
                        paymentProfiel.createCustomerPaymentProfileRequest.merchantAuthentication = mam;
                        paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile = payPro;
                        paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo = btm;
                        paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.payment = payprofileModel;
                        paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.payment.creditCard = creditModel;

                        //Mapping
                        paymentProfiel.createCustomerPaymentProfileRequest.merchantAuthentication.name = LoginId;
                        paymentProfiel.createCustomerPaymentProfileRequest.merchantAuthentication.transactionKey = TransactionKey;
                        paymentProfiel.createCustomerPaymentProfileRequest.customerProfileId = currentProfile.CustomerProfileId;

                        if (address != null)
                        {
                            var firstName = address.FirstName;
                            var lastName = address.LastName;
                            var email = address.Email;
                            var Address1 = address.Address1;
                            var city = address.City;
                            var state = address.StateProvince.Abbreviation.ToString();
                            var country = address.Country.TwoLetterIsoCode;
                            var PhoneNumber = address.PhoneNumber;
                            var ZipPostalCode = address.ZipPostalCode;

                            if (address.FirstName?.Length > 50)
                                firstName = address.FirstName.Substring(0, 50);
                            if (address.LastName?.Length > 50)
                                lastName = address.LastName.Substring(0, 50);
                            if (address.Email?.Length > 20)
                                email = address.Email.Substring(0, 20);
                            if (address.Address1?.Length > 50)
                                Address1 = address.Address1.Substring(0, 50);

                            if (address.StateProvinceId.ToString().Length > 40)
                                Address1 = address.StateProvinceId.ToString().Substring(0, 40);


                            if (address.Country?.TwoLetterIsoCode.Length > 40)
                                country = address.Country.TwoLetterIsoCode.Substring(0, 40);

                            if (address.City?.Length > 40)
                                city = address.City.Substring(0, 40);
                            if (address.ZipPostalCode?.Length > 20)
                                ZipPostalCode = address.ZipPostalCode.Substring(0, 20);

                            if (address.PhoneNumber?.Length > 20)
                                PhoneNumber = address.PhoneNumber.Substring(0, 20);

                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.firstName = firstName;
                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.lastName = lastName;
                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.address = Address1;
                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.city = city;
                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.state = state;
                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.zip = ZipPostalCode;
                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.country = country;
                            paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.billTo.phoneNumber = PhoneNumber;
                        }

                        paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.payment.creditCard.cardNumber = processPaymentRequest.CreditCardNumber;
                        paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.payment.creditCard.expirationDate = processPaymentRequest.CreditCardExpireYear.ToString() + "-" + processPaymentRequest.CreditCardExpireMonth.ToString();
                        paymentProfiel.createCustomerPaymentProfileRequest.paymentProfile.defaultPaymentProfile = false;
                        paymentProfiel.createCustomerPaymentProfileRequest.validationMode = creditCardVaultAuthorizeNetSettings.ValidationMode;

                        ResponseModel responsePayment = new ResponseModel();
                        responsePayment = _customerAuthorizeNetService.CreateCustomerPayment(paymentProfiel);
                        CustomerPaymentProfile newPayment = new CustomerPaymentProfile();

                        if (responsePayment.messages.resultCode == "Error")
                        {
                            _logger.Warning("currentProfile" + responsePayment.messages.message[0].text);

                            if (responsePayment.messages.message[0].code == "E00039")
                            {
                                messageStatus = responsePayment.messages?.message[0]?.text;
                            }

                            newPayment.CustomerProfileId = "Error";
                            newPayment.CustomerPaymentProfileList = responsePayment.messages.message[0].text;
                            newPayment.ResultCode = responsePayment.messages.resultCode;
                            newPayment.CustomerId = currentCustomer.Id;
                            
                            messageStatus = responsePayment.messages?.message[0]?.text;
                        }
                        else
                        {
                            newPayment.CustomerProfileId = responsePayment.customerProfileId;
                            newPayment.CustomerPaymentProfileList = responsePayment.customerPaymentProfileId;
                            newPayment.ResultCode = responsePayment.messages.resultCode;
                            newPayment.CustomerId = currentCustomer.Id;
                            newPayment.BillingAddressId = address.Id;
                            messageStatus = responsePayment.messages?.message[0]?.text;

                            _customerAuthorizeNet.InsertPaymentProfile(newPayment);
                        }

                        return messageStatus;

                    }
                }
            }
            catch (Exception ex)
            {
                _logger.InsertLog(Core.Domain.Logging.LogLevel.Error, ex.Message, ex.StackTrace);
            }
            return messageStatus;
        }

        #endregion
    }
}
