using Nop.Core;
using Nop.Core.Domain.Localization;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Tasks;
using System;
using System.Linq;

namespace Nop.Services.Messages
{
    /// <summary>
    /// Represents a task for sending queued message 
    /// </summary>
    public partial class SendShoppingCartAwaitingCheckoutTask : IScheduleTask
    {
        #region Fields

        private readonly ILogger _logger;
        private readonly IStoreContext _storeContext;
        private readonly LocalizationSettings _localizationSettings;
        private readonly IWorkflowMessageService _workflowMessageService;
        private readonly ICustomerService _customerService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILocalizationService _localizationService;

        #endregion

        #region Ctor

        public SendShoppingCartAwaitingCheckoutTask(
            ILogger logger,
            IStoreContext storeContext,
            LocalizationSettings localizationSettings,
            IWorkflowMessageService workflowMessageService,
            IQueuedEmailService queuedEmailService,
            ICustomerService customerService,
            ILocalizationService localizationService)
        {
            _logger = logger;

            _storeContext = storeContext;
            _localizationSettings = localizationSettings;
            _workflowMessageService = workflowMessageService;
            _customerService = customerService;
            _queuedEmailService = queuedEmailService;
            _localizationService = localizationService;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Executes a task
        /// </summary>
        public virtual void Execute()
        {
            string shortDateString = DateTime.Now.AddDays(-1).ToShortDateString();
            DateTime updateDate = Convert.ToDateTime(shortDateString);
            var TimeHourSend = Convert.ToInt32(_localizationService.GetResource("ShoppingCart.AwaitingHourToSendAfterUpdateCart"));
            //get customers with shopping carts
            var customers = _customerService.GetCustomersWithShoppingCarts(Core.Domain.Orders.ShoppingCartType.ShoppingCart,
                storeId: _storeContext.CurrentStore.Id,
                updatedFromUtc: updateDate);

           
            foreach (var customer in customers.Where(x => x.Email != null && x.NetsuitId == 0).ToList())
            {
                try
                {
                    var ListEmailSend = _queuedEmailService.SearchEmails(customer.Email).Where(r => r.Subject.Contains(_localizationService.GetResource("ShoppingCart.AwaitingCheckoutTemplate"))).OrderByDescending(qe => qe.Id).FirstOrDefault();
                    var getShoppingCartItems = customer.ShoppingCartItems.OrderByDescending(r => r.UpdatedOnUtc).FirstOrDefault();

                    if (ListEmailSend != null)
                    {
                        if (ListEmailSend.SentOnUtc < getShoppingCartItems.UpdatedOnUtc)
                        {
                            //var HourSendAfter = getShoppingCartItems.UpdatedOnUtc.AddHours(TimeHourSend);
                            //if (ListEmailSend.SentOnUtc < HourSendAfter)
                            //{
                            var HourSendAfter = getShoppingCartItems.UpdatedOnUtc.AddHours(TimeHourSend);

                            if (HourSendAfter < DateTime.Now)
                            {
                                // notifications Send Email to Customer
                                _workflowMessageService.SendCustomerShoppingCartAwaitingMessage(customer, _localizationSettings.DefaultAdminLanguageId);

                            }
                        }
                    }
                    else
                    {
                        var HourSendAfter = getShoppingCartItems.UpdatedOnUtc.AddHours(TimeHourSend);

                        if (HourSendAfter < DateTime.Now)
                        {
                            // notifications Send Email to Customer
                            _workflowMessageService.SendCustomerShoppingCartAwaitingMessage(customer, _localizationSettings.DefaultAdminLanguageId);
                        }
                    }
                }
                catch (Exception exc)
                {
                    _logger.Error($"Error notifications Send Email to Customer. {exc.Message}", exc);
                }
            }
        }

        #endregion
    }
}