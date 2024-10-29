using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Tasks;
using System;
using System.Collections.Generic;
using System.Text;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services.Common
{
    public class ImportDataCustomersTask : IScheduleTask
    {
        #region Fields

        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IImportManagerService _importManager;

        #endregion

        #region Ctor

        public ImportDataCustomersTask(INotificationService notificationService,
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
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.ImportDataFromNetsuite);

                if (importer != null)
                {
                    importer.LastExecutionDate = DateTime.UtcNow;
                    _importManager.UpdateImporter(importer);
                    _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Importer.Customers"));
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
