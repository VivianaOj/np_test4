using System;
using System.Collections.Generic;
using System.Linq;
using AuthorizeNet.Api.Contracts.V1;
using AuthorizeNet.Api.Controllers;
using AuthorizeNet.Api.Controllers.Bases;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Plugin.Payments.AuthorizeNet.Controllers;
using Nop.Plugin.Payments.AuthorizeNet.Models;
using Nop.Plugin.Payments.AuthorizeNet.Validators;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using Nop.Services.Security;

using AuthorizeNetSDK = AuthorizeNet;

namespace Nop.Plugin.Payments.AuthorizeNet
{
    /// <summary>
    /// AuthorizeNet payment processor
    /// </summary>
    public class AuthorizeNetPaymentProcessor : BasePlugin, IPaymentMethod
    {
        #region Fields

        private readonly AuthorizeNetPaymentSettings _authorizeNetPaymentSettings;
        private readonly CurrencySettings _currencySettings;
        private readonly ICurrencyService _currencyService;
        private readonly ICustomerService _customerService;
        private readonly IEncryptionService _encryptionService;
        private readonly ILocalizationService _localizationService;
        private readonly ILogger _logger;
        private readonly IOrderProcessingService _orderProcessingService;
        private readonly IOrderService _orderService;
        private readonly IPaymentService _paymentService;
        private readonly ISettingService _settingService;
        private readonly IWebHelper _webHelper;


        #endregion

        #region Ctor

        public AuthorizeNetPaymentProcessor(AuthorizeNetPaymentSettings authorizeNetPaymentSettings,
            CurrencySettings currencySettings,
            ICurrencyService currencyService,
            ICustomerService customerService,
            IEncryptionService encryptionService,
            ILocalizationService localizationService,
            ILogger logger,
            IOrderProcessingService orderProcessingService,
            IOrderService orderService,
            IPaymentService paymentService,
            ISettingService settingService,
            IWebHelper webHelper,
            IWorkContext workContext
            )
        {
            _authorizeNetPaymentSettings = authorizeNetPaymentSettings;
            _currencySettings = currencySettings;
            _currencyService = currencyService;
            _customerService = customerService;
            _encryptionService = encryptionService;
            _localizationService = localizationService;
            _logger = logger;
            _orderProcessingService = orderProcessingService;
            _orderService = orderService;
            _paymentService = paymentService;
            _settingService = settingService;
            _webHelper = webHelper;
         
        }

        #endregion

        #region Utilities

        private static createTransactionResponse GetApiResponse(createTransactionController controller, IList<string> errors)
        {
            var response = controller.GetApiResponse();
            
            if (response != null)
            {
                if (response.transactionResponse?.errors != null)
                {
                    foreach (var transactionResponseError in response.transactionResponse.errors)
                    {
                        errors.Add($"Error #{transactionResponseError.errorCode}: {transactionResponseError.errorText}");
                    }

                    return null;
                }

                if (response.transactionResponse != null && response.messages.resultCode == messageTypeEnum.Ok)
                {
                    switch (response.transactionResponse.responseCode)
                    {
                        case "1":
                            return response;
                        case "2":
                            var description = response.transactionResponse.messages.Any()
                                ? response.transactionResponse.messages.First().description
                                : string.Empty;
                            errors.Add($"Declined ({response.transactionResponse.responseCode}: {description})".TrimEnd(':', ' '));
                            return null;
                    }
                }
                else if (response.transactionResponse != null && response.messages.resultCode == messageTypeEnum.Error)
                {
                    if (response.messages?.message != null && response.messages.message.Any())
                    {
                        var message = response.messages.message.First();

                        errors.Add($"Error #{message.code}: {message.text}");
                        return null;
                    }
                }
            }
            else
            {
                var error = controller.GetErrorResponse();
                if (error?.messages?.message != null && error.messages.message.Any())
                {
                    var message = error.messages.message.First();

                    errors.Add($"Error #{message.code}: {message.text}");
                    return null;
                }
            }

            var controllerResult = controller.GetResults().FirstOrDefault();

            if (controllerResult?.StartsWith("I00001", StringComparison.InvariantCultureIgnoreCase) ?? false)
                return null;

            const string unknownError = "Authorize.NET unknown error";
            errors.Add(string.IsNullOrEmpty(controllerResult) ? unknownError : $"{unknownError} ({controllerResult})");
            
            return null;
        }

        private static customerAddressType GetTransactionRequestAddress(Address address)
        {
            var firstName = address.FirstName;
            var lastName = address.LastName;
            var email = address.Email;
            var Address1 = address.Address1;
            var city = address.City;
            var ZipPostalCode = address.ZipPostalCode;
            var Company = address.Company;

            if (address.FirstName?.Length > 50)
                firstName = address.FirstName.Substring(0, 50);
            if (address.LastName?.Length > 50)
                lastName = address.LastName.Substring(0, 50);
            if (address.Email?.Length > 20)
                email = address.Email.Substring(0, 20);
            if (address.Address1?.Length > 50)
                Address1 = address.Address1.Substring(0, 50);
            if (address.City?.Length > 40)
                city = address.City.Substring(0, 40);
            if (address.ZipPostalCode?.Length > 20)
                ZipPostalCode = address.ZipPostalCode.Substring(0, 20);

            if (address.Company?.Length > 50)
                Company = address.Company.Substring(0, 50);

            var transactionRequestAddress = new customerAddressType
            {
                firstName = firstName,
                lastName = lastName,
                email = email,
                address = Address1,
                city = city,
                zip = ZipPostalCode
            };

            if (!string.IsNullOrEmpty(address.Company))
                transactionRequestAddress.company = Company;

            if (address.StateProvince != null)
                transactionRequestAddress.state = address.StateProvince.Abbreviation;

            if (address.Country != null)
                transactionRequestAddress.country = address.Country.TwoLetterIsoCode;

            return transactionRequestAddress;
        }

        private static nameAndAddressType GetRecurringTransactionRequestAddress(Address address)
        {
            var transactionRequestAddress = new nameAndAddressType
            {
                firstName = address.FirstName,
                lastName = address.LastName,
                address = address.Address1,
                city = address.City,
                zip = address.ZipPostalCode
            };

            if (!string.IsNullOrEmpty(address.Company))
                transactionRequestAddress.company = address.Company;

            if (address.StateProvince != null)
                transactionRequestAddress.state = address.StateProvince.Abbreviation;

            if (address.Country != null)
                transactionRequestAddress.country = address.Country.TwoLetterIsoCode;

            return transactionRequestAddress;
        }

        private void PrepareAuthorizeNet()
        {
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.RunEnvironment = _authorizeNetPaymentSettings.UseSandbox
                ? AuthorizeNetSDK.Environment.SANDBOX
                : AuthorizeNetSDK.Environment.PRODUCTION;

            // define the merchant information (authentication / transaction id)
            ApiOperationBase<ANetApiRequest, ANetApiResponse>.MerchantAuthentication = new merchantAuthenticationType
            {
                name = _authorizeNetPaymentSettings.LoginId,
                ItemElementName = ItemChoiceType.transactionKey,
                Item = _authorizeNetPaymentSettings.TransactionKey
            };
        }

        protected virtual TransactionDetails GetTransactionDetails(string transactionId)
        {
            PrepareAuthorizeNet();

            var request = new getTransactionDetailsRequest {transId = transactionId};

            // instantiate the controller that will call the service
            var controller = new getTransactionDetailsController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            if (response?.messages == null)
            {
                _logger.Error($"Authorize.NET unknown error (transactionId: {transactionId})");

                return new TransactionDetails { IsOk = false };
            }

            var transactionDetails = new TransactionDetails
            {
                IsOk = response.messages.resultCode == messageTypeEnum.Ok,
                Message = response.messages.message.FirstOrDefault()
            };

            if (response.transaction == null)
            {
                _logger.Error($"Authorize.NET: Transaction data is missing (transactionId: {transactionId})");
            }
            else
            {
                transactionDetails.TransactionStatus = response.transaction.transactionStatus;
                transactionDetails.OrderDescriptions = response.transaction.order.description.Split('#');
                transactionDetails.TransactionType = response.transaction.transactionType;
            }

            return transactionDetails;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            PrepareAuthorizeNet();

            var creditCard = new creditCardType
            {
                cardNumber = processPaymentRequest.CreditCardNumber,
                expirationDate =
                    processPaymentRequest.CreditCardExpireMonth?.ToString() + processPaymentRequest.CreditCardExpireYear,
                cardCode = processPaymentRequest.CreditCardCvv2
            };
            var profileInfo = new customerProfilePaymentType();
            if (processPaymentRequest.customerProfileId != null && processPaymentRequest.paymentProfileId != null)
            {
                profileInfo = new customerProfilePaymentType
                {
                    customerProfileId = processPaymentRequest.customerProfileId,
                    paymentProfile = new paymentProfile
                    {
                        paymentProfileId = processPaymentRequest.paymentProfileId
                    }
                };
            }
            

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            transactionTypeEnum transactionType;

            switch (_authorizeNetPaymentSettings.TransactMode)
            {
                case TransactMode.Authorize:
                    transactionType = transactionTypeEnum.authOnlyTransaction;
                    break;
                case TransactMode.AuthorizeAndCapture:
                    transactionType = transactionTypeEnum.authCaptureTransaction;
                    break;
                default:
                    throw new NopException("Not supported transaction mode");
            }

            var billTo = _authorizeNetPaymentSettings.UseShippingAddressAsBilling && customer.ShippingAddress != null ? GetTransactionRequestAddress(customer.ShippingAddress) : GetTransactionRequestAddress(customer.BillingAddress);
            
            if (processPaymentRequest.NewAddress != null)
            {
                billTo.address = processPaymentRequest.NewAddress.Address1;
                billTo.city = processPaymentRequest.NewAddress.City;
                billTo.phoneNumber = processPaymentRequest.NewAddress.PhoneNumber;
                billTo.zip = processPaymentRequest.NewAddress.ZipPostalCode;
            }
            
            var transactionRequest = new transactionRequestType();
            if (profileInfo.customerProfileId != null && profileInfo.paymentProfile?.paymentProfileId != null)
            {
                transactionRequest = new transactionRequestType
                {
                    transactionType = transactionType.ToString(),
                    amount = processPaymentRequest.OrderTotal,
                    profile = profileInfo,
                    currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
                    customerIP = _webHelper.GetCurrentIpAddress(),
                    order = new orderType
                    {
                        //x_invoice_num is 20 chars maximum. Hence we also pass x_description
                        invoiceNumber = processPaymentRequest.OrderGuid.ToString().Substring(0, 20),
                        description = $"Full order #{processPaymentRequest.OrderGuid}"
                    }
                };
            }
            else
            {
                transactionRequest = new transactionRequestType
                {
                    transactionType = transactionType.ToString(),
                    amount = processPaymentRequest.OrderTotal,
                    payment = paymentType,
                    currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,
                    billTo = billTo,
                    customerIP = _webHelper.GetCurrentIpAddress(),
                    order = new orderType
                    {
                        //x_invoice_num is 20 chars maximum. Hence we also pass x_description
                        invoiceNumber = processPaymentRequest.OrderGuid.ToString().Substring(0, 20),
                        description = $"Full order #{processPaymentRequest.OrderGuid}"
                    }
                };
            }

            if (customer.ShippingAddress != null && !_authorizeNetPaymentSettings.UseShippingAddressAsBilling)
            {
                var shipTo = GetTransactionRequestAddress(customer.ShippingAddress);
                transactionRequest.shipTo = shipTo;
            }
            var billingInfo = JsonConvert.SerializeObject(transactionRequest.billTo);
            var shippingInfo = JsonConvert.SerializeObject(transactionRequest.shipTo);
            _logger.Information($"Authorize.NET Billing: {billingInfo + "--shippingInfo:--" + shippingInfo})");
             var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = GetApiResponse(controller, result.Errors);

            //validate
            if (response == null)
                return result;

            if (_authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize)
            {
                result.AuthorizationTransactionId = response.transactionResponse.transId;
                result.AuthorizationTransactionCode =
                    $"{response.transactionResponse.transId},{response.transactionResponse.authCode}";
            }

            if (_authorizeNetPaymentSettings.TransactMode == TransactMode.AuthorizeAndCapture)
                result.CaptureTransactionId =
                    $"{response.transactionResponse.transId},{response.transactionResponse.authCode}";

            result.AuthorizationTransactionResult =
                $"Approved ({response.transactionResponse.responseCode}: {response.transactionResponse.messages[0].description})";
            result.AvsResult = response.transactionResponse.avsResultCode;
            result.NewPaymentStatus = _authorizeNetPaymentSettings.TransactMode == TransactMode.Authorize ? PaymentStatus.Authorized : PaymentStatus.Paid;

            return result;
        }

        /// <summary>
        /// Post process payment (used by payment gateways that require redirecting to a third-party URL)
        /// </summary>
        /// <param name="postProcessPaymentRequest">Payment info required for an order processing</param>
        public void PostProcessPayment(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            //nothing
        }

        /// <summary>
        /// Gets additional handling fee
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>Additional handling fee</returns>
        public decimal GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            var result = _paymentService.CalculateAdditionalFee(cart,
                _authorizeNetPaymentSettings.AdditionalFee, _authorizeNetPaymentSettings.AdditionalFeePercentage);
            return result;
        }

        /// <summary>
        /// Returns a value indicating whether payment method should be hidden during checkout
        /// </summary>
        /// <param name="cart">Shopping cart</param>
        /// <returns>true - hide; false - display.</returns>
        public bool HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            //you can put any logic here
            //for example, hide this payment method if all products in the cart are downloadable
            //or hide this payment method if current customer is from certain country
            return false;
        }
        
        /// <summary>
        /// Captures payment
        /// </summary>
        /// <param name="capturePaymentRequest">Capture payment request</param>
        /// <returns>Capture payment result</returns>
        public CapturePaymentResult Capture(CapturePaymentRequest capturePaymentRequest)
        {
            var result = new CapturePaymentResult();

            PrepareAuthorizeNet();

            var codes = capturePaymentRequest.Order.AuthorizationTransactionCode.Split(',');
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.priorAuthCaptureTransaction.ToString(),
                amount = Math.Round(capturePaymentRequest.Order.OrderTotal, 2),
                refTransId = codes[0],
                currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            //instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            //get the response from the service (errors contained if any)
            var response = GetApiResponse(controller, result.Errors);

            //validate
            if (response == null)
                return result;

            result.CaptureTransactionId =
                $"{response.transactionResponse.transId},{response.transactionResponse.authCode}";
            result.CaptureTransactionResult =
                $"Approved ({response.transactionResponse.responseCode}: {response.transactionResponse.messages[0].description})";
            result.NewPaymentStatus = PaymentStatus.Paid;

            return result;
        }
        
        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public RefundPaymentResult Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            var codes = (string.IsNullOrEmpty(refundPaymentRequest.Order.CaptureTransactionId)
                ? refundPaymentRequest.Order.AuthorizationTransactionCode
                : refundPaymentRequest.Order.CaptureTransactionId).Split(',');

            var transactionId = codes[0];

            var transactionDetails = GetTransactionDetails(transactionId);

            if (transactionDetails.TransactionStatus == "capturedPendingSettlement")
            {
                if (refundPaymentRequest.IsPartialRefund)
                {
                    result.Errors.Add("This transaction is supports only full refund");

                    return result;
                }

                var voidResult = Void(new VoidPaymentRequest
                {
                    Order = refundPaymentRequest.Order
                });

                if (!voidResult.Success)
                {
                    foreach (var voidResultError in voidResult.Errors)
                    {
                        result.Errors.Add(voidResultError);
                    }

                    return result;
                }

                result.NewPaymentStatus = PaymentStatus.Refunded;

                return result;
            }
            
            PrepareAuthorizeNet();

            var maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(refundPaymentRequest.Order.MaskedCreditCardNumber);

            if (string.IsNullOrEmpty(maskedCreditCardNumberDecrypted) || maskedCreditCardNumberDecrypted.Length < 4)
            {
                result.AddError("Last four digits of Credit Card Not Available");
                return result;
            }

            var lastFourDigitsCardNumber = maskedCreditCardNumberDecrypted.Substring(maskedCreditCardNumberDecrypted.Length - 4);
            var creditCard = new creditCardType
            {
                cardNumber = lastFourDigitsCardNumber,
                expirationDate = "XXXX"
            };

            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.refundTransaction.ToString(),
                amount = Math.Round(refundPaymentRequest.AmountToRefund, 2),
                refTransId = transactionId,
                currencyCode = _currencyService.GetCurrencyById(_currencySettings.PrimaryStoreCurrencyId).CurrencyCode,

                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. Hence we also pass x_description
                    invoiceNumber = refundPaymentRequest.Order.OrderGuid.ToString().Substring(0, 20),
                    description = $"Full order #{refundPaymentRequest.Order.OrderGuid}"
                },

                payment = new paymentType { Item = creditCard }
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            GetApiResponse(controller, result.Errors);
            result.NewPaymentStatus = PaymentStatus.PartiallyRefunded;

            return result;
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <param name="voidPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public VoidPaymentResult Void(VoidPaymentRequest voidPaymentRequest)
        {
            var result = new VoidPaymentResult();

            PrepareAuthorizeNet();

            var maskedCreditCardNumberDecrypted = _encryptionService.DecryptText(voidPaymentRequest.Order.MaskedCreditCardNumber);

            if (string.IsNullOrEmpty(maskedCreditCardNumberDecrypted) || maskedCreditCardNumberDecrypted.Length < 4)
            {
                result.AddError("Last four digits of Credit Card Not Available");
                return result;
            }

            var lastFourDigitsCardNumber = maskedCreditCardNumberDecrypted.Substring(maskedCreditCardNumberDecrypted.Length - 4);
            var expirationDate = voidPaymentRequest.Order.CardExpirationMonth + voidPaymentRequest.Order.CardExpirationYear;

            if (!expirationDate.Any())
                expirationDate = "XXXX";

            var creditCard = new creditCardType
            {
                cardNumber = lastFourDigitsCardNumber,
                expirationDate = expirationDate
            };

            var codes = (string.IsNullOrEmpty(voidPaymentRequest.Order.CaptureTransactionId) ? voidPaymentRequest.Order.AuthorizationTransactionCode : voidPaymentRequest.Order.CaptureTransactionId).Split(',');
            var transactionRequest = new transactionRequestType
            {
                transactionType = transactionTypeEnum.voidTransaction.ToString(),
                refTransId = codes[0],
                payment = new paymentType { Item = creditCard }
            };

            var request = new createTransactionRequest { transactionRequest = transactionRequest };

            // instantiate the controller that will call the service
            var controller = new createTransactionController(request);
            controller.Execute();

            var response = GetApiResponse(controller, result.Errors);

            //validate
            if (response == null)
                return result;

            result.NewPaymentStatus = PaymentStatus.Voided;

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="processPaymentRequest">Payment info required for an order processing</param>
        /// <returns>Process payment result</returns>
        public ProcessPaymentResult ProcessRecurringPayment(ProcessPaymentRequest processPaymentRequest)
        {
            var result = new ProcessPaymentResult();
           
            var customer = _customerService.GetCustomerById(processPaymentRequest.CustomerId);

            PrepareAuthorizeNet();
            
            var creditCard = new creditCardType
            {
                cardNumber = processPaymentRequest.CreditCardNumber,
                expirationDate =
                    processPaymentRequest.CreditCardExpireMonth.ToString() + processPaymentRequest.CreditCardExpireYear,
                cardCode = processPaymentRequest.CreditCardCvv2
            };

            //standard api call to retrieve response
            var paymentType = new paymentType { Item = creditCard };

            var billTo = _authorizeNetPaymentSettings.UseShippingAddressAsBilling && customer.ShippingAddress != null ? GetRecurringTransactionRequestAddress(customer.ShippingAddress) : GetRecurringTransactionRequestAddress(customer.BillingAddress);

            var dtNow = DateTime.UtcNow;

            // Interval can't be updated once a subscription is created.
            var paymentScheduleInterval = new paymentScheduleTypeInterval();
            switch (processPaymentRequest.RecurringCyclePeriod)
            {
                case RecurringProductCyclePeriod.Days:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.days;
                    break;
                case RecurringProductCyclePeriod.Weeks:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 7);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.days;
                    break;
                case RecurringProductCyclePeriod.Months:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.months;
                    break;
                case RecurringProductCyclePeriod.Years:
                    paymentScheduleInterval.length = Convert.ToInt16(processPaymentRequest.RecurringCycleLength * 12);
                    paymentScheduleInterval.unit = ARBSubscriptionUnitEnum.months;
                    break;
                default:
                    throw new NopException("Not supported cycle period");
            }

            var paymentSchedule = new paymentScheduleType
            {
                startDate = new DateTime(dtNow.Year, dtNow.Month, dtNow.Day),
                totalOccurrences = Convert.ToInt16(processPaymentRequest.RecurringTotalCycles),
                interval = paymentScheduleInterval
            };
            
            var subscriptionType = new ARBSubscriptionType
            {
                name = processPaymentRequest.OrderGuid.ToString(),
                amount = Math.Round(processPaymentRequest.OrderTotal, 2),
                payment = paymentType,
                billTo = billTo,
                paymentSchedule = paymentSchedule,
                customer = new customerType
                {
                    email = customer.BillingAddress.Email
                    //phone number should be in one of following formats: 111- 111-1111 or (111) 111-1111.
                    //phoneNumber = customer.BillingAddress.PhoneNumber
                },
                
                order = new orderType
                {
                    //x_invoice_num is 20 chars maximum. Hence we also pass x_description
                    invoiceNumber = processPaymentRequest.OrderGuid.ToString().Substring(0, 20),
                    description = $"Recurring payment #{processPaymentRequest.OrderGuid}"
                }
            };

            if (customer.ShippingAddress != null && !_authorizeNetPaymentSettings.UseShippingAddressAsBilling)
            {
                var shipTo = GetRecurringTransactionRequestAddress(customer.ShippingAddress);
                subscriptionType.shipTo = shipTo;
            }

            var request = new ARBCreateSubscriptionRequest { subscription = subscriptionType };

            // instantiate the controller that will call the service
            var controller = new ARBCreateSubscriptionController(request);
            controller.Execute();

            // get the response from the service (errors contained if any)
            var response = controller.GetApiResponse();

            //validate
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
            {
                result.SubscriptionTransactionId = response.subscriptionId;
                result.AuthorizationTransactionCode = response.refId;
                result.AuthorizationTransactionResult = $"Approved ({response.refId}: {response.subscriptionId})";
            }
            else if (response != null)
            {
                foreach (var responseMessage in response.messages.message)
                {
                    result.AddError(
                        $"Error processing recurring payment #{responseMessage.code}: {responseMessage.text}");
                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }

            return result;
        }

        /// <summary>
        /// Process recurring payment
        /// </summary>
        /// <param name="transactionId">AuthorizeNet transaction ID</param>
        public void ProcessRecurringPayment(string transactionId)
        {
            var transactionDetails = GetTransactionDetails(transactionId);

            if (transactionDetails.TransactionStatus == "refundTransaction")
            {
                return;
            }

            var orderDescriptions = transactionDetails.OrderDescriptions;

            if (orderDescriptions.Length < 2)
            {
                _logger.Error($"Authorize.NET: Missing order GUID (transactionId: {transactionId})");
                return;
            }

            if (orderDescriptions[0].Contains("Full order"))
                return;

            var order = _orderService.GetOrderByGuid(new Guid(orderDescriptions[1]));

            if (order == null)
            {
                _logger.Error($"Authorize.NET: Order cannot be loaded (order GUID: {orderDescriptions[1]})");
                return;
            }

            var recurringPayments = _orderService.SearchRecurringPayments(initialOrderId: order.Id);

            foreach (var rp in recurringPayments)
            {
                if (transactionDetails.IsOk)
                {
                    var recurringPaymentHistory = rp.RecurringPaymentHistory;
                    var orders = _orderService.GetOrdersByIds(recurringPaymentHistory.Select(rph => rph.OrderId).ToArray()).ToList();

                    var transactionsIds = new List<string>();
                    transactionsIds.AddRange(orders.Select(o => o.AuthorizationTransactionId).Where(tId => !string.IsNullOrEmpty(tId)));
                    transactionsIds.AddRange(orders.Select(o => o.CaptureTransactionId).Where(tId => !string.IsNullOrEmpty(tId)));

                    //skip the re-processing of transactions
                    if (transactionsIds.Contains(transactionId))
                        continue;

                    var newPaymentStatus = transactionDetails.TransactionType == "authOnlyTransaction" ? PaymentStatus.Authorized : PaymentStatus.Paid;

                    if (recurringPaymentHistory.Count == 0)
                    {
                        //first payment
                        var rph = new RecurringPaymentHistory
                        {
                            RecurringPaymentId = rp.Id,
                            OrderId = order.Id,
                            CreatedOnUtc = DateTime.UtcNow
                        };
                        rp.RecurringPaymentHistory.Add(rph);

                        if (newPaymentStatus == PaymentStatus.Authorized)
                            rp.InitialOrder.AuthorizationTransactionId = transactionId;
                        else
                            rp.InitialOrder.CaptureTransactionId = transactionId;

                        _orderService.UpdateRecurringPayment(rp);
                    }
                    else
                    {
                        //next payments
                        var processPaymentResult = new ProcessPaymentResult
                        {
                            NewPaymentStatus = newPaymentStatus
                        };

                        if (newPaymentStatus == PaymentStatus.Authorized)
                            processPaymentResult.AuthorizationTransactionId = transactionId;
                        else
                            processPaymentResult.CaptureTransactionId = transactionId;

                        _orderProcessingService.ProcessNextRecurringPayment(rp, processPaymentResult);
                    }
                }
                else
                {
                    var processPaymentResult = new ProcessPaymentResult();
                    processPaymentResult.AuthorizationTransactionId = processPaymentResult.CaptureTransactionId = transactionId;
                    processPaymentResult.RecurringPaymentFailed = true;
                    processPaymentResult.Errors.Add(
                        $"Authorize.Net Error: {transactionDetails.Message?.code} - {transactionDetails.Message?.text} (transactionId: {transactionId})");
                    _orderProcessingService.ProcessNextRecurringPayment(rp, processPaymentResult);
                }
            }
        }

        /// <summary>
        /// Cancels a recurring payment
        /// </summary>
        /// <param name="cancelPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public CancelRecurringPaymentResult CancelRecurringPayment(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            var result = new CancelRecurringPaymentResult();
            PrepareAuthorizeNet();

            var request = new ARBCancelSubscriptionRequest { subscriptionId = cancelPaymentRequest.Order.SubscriptionTransactionId };
            var controller = new ARBCancelSubscriptionController(request);
            controller.Execute();

            var response = controller.GetApiResponse();

            //validate
            if (response != null && response.messages.resultCode == messageTypeEnum.Ok)
                return result;

            if (response != null)
            {
                foreach (var responseMessage in response.messages.message)
                {
                    result.AddError(
                        $"Error processing recurring payment #{responseMessage.code}: {responseMessage.text}");
                }
            }
            else
            {
                result.AddError("Authorize.NET unknown error");
            }
           
            return result;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="order">Order</param>
        /// <returns>Result</returns>
        public bool CanRePostProcessPayment(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));
            
            //it's not a redirection payment method. So we always return false
            return false;
        }

        /// <summary>
        /// Validate payment form
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>List of validating errors</returns>
        public IList<string> ValidatePaymentForm(IFormCollection form)
        {
            var warnings = new List<string>();

            //validate
            var validator = new PaymentInfoValidator(_localizationService);
            var model = new PaymentInfoModel
            {
                CardholderName = form["CardholderName"],
                CardNumber = form["CardNumber"],
                CardCode = form["CardCode"],
                ExpireMonth = form["ExpireMonth"],
                ExpireYear = form["ExpireYear"]
            };

            var validationResult = validator.Validate(model);

            if (!validationResult.IsValid)
                warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));

            return warnings;
        }

        /// <summary>
        /// Get payment information
        /// </summary>
        /// <param name="form">The parsed form values</param>
        /// <returns>Payment info holder</returns>
        public ProcessPaymentRequest GetPaymentInfo(IFormCollection form)
        {
            var paymentInfo = new ProcessPaymentRequest
            {
                //CreditCardType is not used by Authorize.NET
                CreditCardName = form["CardholderName"],
                CreditCardNumber = form["CardNumber"],
                CreditCardExpireMonth = form["ExpireMonth"],
                CreditCardExpireYear = int.Parse(form["ExpireYear"]),
                CreditCardCvv2 = form["CardCode"],
            };


            return paymentInfo;
        }

        /// <summary>
        /// Gets a view component for displaying plugin in public store ("payment info" checkout step)
        /// </summary>
        public string GetPublicViewComponentName()
        {
            return "AuthorizeNet";
        }

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/PaymentAuthorizeNet/Configure";
        }

        /// <summary>
        /// Install plugin
        /// </summary>
        public override void Install()
        {
            //settings
            var settings = new AuthorizeNetPaymentSettings
            {
                UseSandbox = true,
                TransactMode = TransactMode.Authorize,
                TransactionKey = "123",
                LoginId = "456"
            };
            _settingService.SaveSetting(settings);

            //locales
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Notes", "If you're using this gateway, ensure that your primary store currency is supported by Authorize.NET.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox", "Use Sandbox");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox.Hint", "Check to enable Sandbox (testing environment).");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseShippingAddressAsBilling", "Use shipping address.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseShippingAddressAsBilling.Hint", "Check if you want to use the shipping address as a billing address.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues", "Transaction mode");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues.Hint", "Choose transaction mode.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey", "Transaction key");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey.Hint", "Specify transaction key.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId", "Login ID");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId.Hint", "Specify login identifier.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee", "Additional fee");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee.Hint", "Enter additional fee to charge your customers.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage", "Additional fee. Use percentage");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage.Hint", "Determines whether to apply a percentage additional fee to the order total. If not enabled, a fixed value is used.");
            _localizationService.AddOrUpdatePluginLocaleResource("Plugins.Payments.AuthorizeNet.PaymentMethodDescription", "Pay by credit / debit card");

            base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override void Uninstall()
        {
            //settings
            _settingService.DeleteSetting<AuthorizeNetPaymentSettings>();

            //locales
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Notes");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseSandbox.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseShippingAddressAsBilling");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.UseShippingAddressAsBilling.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactModeValues.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.TransactionKey.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.LoginId.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFee.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.Fields.AdditionalFeePercentage.Hint");
            _localizationService.DeletePluginLocaleResource("Plugins.Payments.AuthorizeNet.PaymentMethodDescription");
            
            base.Uninstall();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public bool SupportCapture => true;

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public bool SupportPartiallyRefund => true;

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public bool SupportRefund => true;

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public bool SupportVoid => true;

        /// <summary>
        /// Gets a recurring payment type of payment method
        /// </summary>
        public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.Automatic;

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public bool SkipPaymentInfo => false;

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public string PaymentMethodDescription => _localizationService.GetResource("Plugins.Payments.AuthorizeNet.PaymentMethodDescription");

        #endregion

        #region Nested classes

        protected partial class TransactionDetails
        {
            public bool IsOk { get; set; }

            public string TransactionStatus { get; internal set; }

            public string[] OrderDescriptions { get; internal set; }

            public messagesTypeMessage Message { get; internal set; }

            public string TransactionType { get; set; }
        }

        #endregion
    }
}
