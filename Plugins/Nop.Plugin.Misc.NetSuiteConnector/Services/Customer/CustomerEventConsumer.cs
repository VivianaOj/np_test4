using Nop.Core.Domain.Customers;
using Nop.Core.Events;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public class CustomerEventConsumer :
        IConsumer<EntityDeletedEvent<Customer>>,
        IConsumer<EntityUpdatedEvent<Customer>>,
        IConsumer<EntityInsertedEvent<Customer>>,
        IConsumer<CustomerLoggedinEvent>
    {
        private readonly IOrderService _orderService;
        private readonly IConnectionServices _connectionSevices;

        public CustomerEventConsumer(IOrderService orderService, IConnectionServices connectionSevices)
        {
            _orderService = orderService;
            _connectionSevices = connectionSevices;
        }

        #region Methods

        public void HandleEvent(EntityDeletedEvent<Customer> eventMessage)
        {

        }

        public void HandleEvent(EntityUpdatedEvent<Customer> eventMessage)
        {

        }

        public void HandleEvent(EntityInsertedEvent<Customer> eventMessage)
        {

        }

        public void HandleEvent(CustomerLoggedinEvent eventMessage)
        {
            //    _orderService.GetTransactionInfoOpenAndPending(eventMessage.Customer);
           
        }

        #endregion
    }
}
