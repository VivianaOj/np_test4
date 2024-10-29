using Nop.Core.Domain.Catalog;
using Nop.Core.Events;
using Nop.Services.Events;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public class ProductEventConsumer :
        IConsumer<EntityDeletedEvent<Product>>,
        IConsumer<EntityUpdatedEvent<Product>>,
        IConsumer<EntityInsertedEvent<Product>>
    {
        #region Methods

        public void HandleEvent(EntityDeletedEvent<Product> eventMessage)
        {
            
        }

        public void HandleEvent(EntityUpdatedEvent<Product> eventMessage)
        {
            
        }

        public void HandleEvent(EntityInsertedEvent<Product> eventMessage)
        {
            
        }

        #endregion
    }
}
