using System;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Tasks;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public class ImportOrdersTask : IScheduleTask
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IImportManagerService _importManager;

        #endregion

        #region Ctor

        public ImportOrdersTask(INotificationService notificationService,
            ILocalizationService localizationService,
            IImportManagerService importManager)
        {
            _notificationService = notificationService;
            _localizationService = localizationService;
            _importManager = importManager;
        }

        #endregion

        #region Methods

        public void Execute()
        {
            try
            {
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.OrderImporter);

                if (importer != null)
                {
                    importer.LastExecutionDate = DateTime.UtcNow;
                    _importManager.UpdateImporter(importer);
                    _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Importer.Orders"));
                }
                else
                {
                    throw new Exception("Importer doesn't exists");
                }
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
            }
        }

        #endregion
    }
}
