using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public class OrderEventConsumer :
        IConsumer<EntityDeletedEvent<Nop.Core.Domain.Orders.Order>>,
        IConsumer<EntityUpdatedEvent<Nop.Core.Domain.Orders.Order>>,
        IConsumer<OrderCancelledEvent>,
        IConsumer<OrderPlacedEvent>,
        IConsumer<OrderRefundedEvent>,
        IConsumer<OrderVoidedEvent>
    {
        private readonly IOrderService _orderService;
        private readonly IConnectionServices _connectionSevices;

        public OrderEventConsumer(IOrderService orderService, IConnectionServices connectionSevices)
        {
            _orderService = orderService;
            _connectionSevices = connectionSevices;
        }
        #region Methods

        public void HandleEvent(EntityDeletedEvent<Nop.Core.Domain.Orders.Order> eventMessage)
        {
            
        }

        public void HandleEvent(EntityUpdatedEvent<Nop.Core.Domain.Orders.Order> eventMessage)
        {
            
        }

        public void HandleEvent(OrderCancelledEvent eventMessage)
        {
            
        }

        public void HandleEvent(OrderPlacedEvent eventMessage)
        {
            _orderService.SendOrderNetsuiteEventCreateOrder(eventMessage.Order);
        }

        public void HandleEvent(OrderRefundedEvent eventMessage)
        {
            
        }

        public void HandleEvent(OrderVoidedEvent eventMessage)
        {
            
        }

        #endregion
    }
}
