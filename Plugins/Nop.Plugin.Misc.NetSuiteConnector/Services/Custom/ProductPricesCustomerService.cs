using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using System;
using Nop.Services.Logging;
using Nop.Services.Messages;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Custom
{
    public class ProductPricesCustomerService : IProductPricesCustomerService
    {
        #region Fields

        private readonly IRepository<ProductPricesCustomer> _productPricesRepository;

        #endregion

        #region Ctor

        public ProductPricesCustomerService(IRepository<ProductPricesCustomer> productPricesRepository)
        {
            _productPricesRepository = productPricesRepository;
        }

        #endregion

        #region Methods

        public void InsertProductPricesCustomer(ProductPricesCustomer productPricesCustomer)
        {
            try
            {

                if (productPricesCustomer == null)
                    throw new ArgumentNullException(nameof(productPricesCustomer));

                _productPricesRepository.Insert(productPricesCustomer);
            }
            catch (Exception ex)
            {
                //Nop.Services.Messages.INotificationService _notService = NopEngine._serviceProvider.GetService<INotificationService>();
                //ILogger _log = NopEngine._serviceProvider.GetService<ILogger>();

                //_notService.ErrorNotification(ex.Message);
                //_log.Warning("Import  " + ex.Message);
            }
        }

        #endregion
    }
}
