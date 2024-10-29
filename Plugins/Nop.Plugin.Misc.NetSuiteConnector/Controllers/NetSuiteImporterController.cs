using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Nop.Core.Domain.Logging;
using Nop.Core.Domain.NN;
using Nop.Plugin.Misc.NetSuiteConnector.Models.NetSuiteImporter;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.NN;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Customers;
using Nop.Web.Framework.Models.Extensions;

namespace Nop.Plugin.Misc.NetSuiteConnector.Controllers
{
    public class NetSuiteImporterController : BaseAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IImportManagerService _importManager;
        private readonly ISettingService _settingService;
        private readonly ILogger _logger;
        private readonly ILogImportNetsuiteService _logImportNetsuite;

        private IPendingDataToSyncService _pendingDataToSyncService;

        #endregion

        #region Ctor

        public NetSuiteImporterController(IPermissionService permissionService, 
            INotificationService notificationService,
            ILocalizationService localizationService,
            IImportManagerService importManager, ISettingService settingService, ILogger logger, ILogImportNetsuiteService logImportNetsuite
            , IPendingDataToSyncService pendingDataToSyncService)
        {
            _permissionService = permissionService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _importManager = importManager;
            _settingService = settingService;
            _logger = logger;
            _logImportNetsuite = logImportNetsuite;
            _pendingDataToSyncService = pendingDataToSyncService;
        }

        #endregion

        #region Methods

        [HttpGet]
        public virtual IActionResult Index()
        {
            var importers = _importManager.GetImporters();
            var model = new List<NetSuiteImporterModel>();

            var productLogs = _logger.GetAllLogs(DateTime.Now.AddMonths(-3), DateTime.Now, "ImportProductError", LogLevel.Warning);

            var listProductLogs = productLogs
                .Where(log => log.ShortMessage.StartsWith("ImportProductError"))
                .Select(log =>
                {
                    var logParts = log.ShortMessage.Split("::");
                    if(logParts.Length > 3)
					{
                        return new LogsImportModel
                        {
                            Name = logParts.Length > 2 ? logParts[1].Trim() + " " + logParts[2].Trim() : null,
                            Link = log.Id,
                            LastExecutionDate = logParts.Length > 1 ? logParts[4].Trim() : null
                        };
					}
					else
					{
                        return new LogsImportModel
                        {
                            Name = logParts.Length > 2 ? logParts[1].Trim() + " " + logParts[2].Trim() : null,
                            Link = log.Id,
                            LastExecutionDate = logParts.Length > 1 ? logParts[2].Trim() : null
                        };

                    }
                    
                })
                .Where(log => log.LastExecutionDate != null)
                .GroupBy(log => log.LastExecutionDate)
                .Select(group => new LogsListByImportDate
                {
                    LastExecutionDate = group.Key,
                    Logs = group.ToList()
                })
                .ToList();

            var customerLogs = _logger.GetAllLogs(DateTime.Now.AddMonths(-3), DateTime.Now, "ImportCustomerError", LogLevel.Warning);

            var listCustomerLogs = customerLogs
                .Where(log => log.ShortMessage.StartsWith("ImportCustomerError"))
                .Select(log =>
                {
                    var logParts = log.ShortMessage.Split("::");

                    return new LogsImportModel
                    {
                        Name = logParts.Length > 2 ? logParts[1].Trim() + " " + logParts[2].Trim() : null,
                        Link = log.Id,
                        LastExecutionDate = logParts.Length > 1 ? logParts[4].Trim() : null
                    };
                })
                .Where(log => log.LastExecutionDate != null)
                .GroupBy(log => log.LastExecutionDate)
                .Select(group => new LogsListByImportDate
                {
                    LastExecutionDate = group.Key,
                    Logs = group.ToList()
                })
                .ToList();

            var OrderLogs = _logger.GetAllLogs(DateTime.Now.AddMonths(-3), DateTime.Now, "ImportOrderError", LogLevel.Warning);

            var listOrderLogs = OrderLogs
                .Where(log => log.ShortMessage.StartsWith("ImportOrderError"))
                .Select(log =>
                {
                    var logParts = log.ShortMessage.Split("::");

                    return new LogsImportModel
                    {
                        Name = logParts.Length > 2 ? logParts[1].Trim() + " " + logParts[2].Trim() : null,
                        Link = log.Id,
                        LastExecutionDate = logParts.Length > 1 ? logParts[4].Trim() : null
                    };
                })
                .Where(log => log.LastExecutionDate != null)
                .GroupBy(log => log.LastExecutionDate)
                .Select(group => new LogsListByImportDate
                {
                    LastExecutionDate = group.Key,
                    Logs = group.ToList()
                })
                .ToList();

            var pendingOrderLogs = _logger.GetAllLogs(DateTime.Now.AddMonths(-3), DateTime.Now, "ImportSendOrderError", LogLevel.Warning);

            var listPendingOrderLogs = pendingOrderLogs
                .Where(log => log.ShortMessage.StartsWith("ImportSendOrderError"))
                .Select(log =>
                {
                    var logParts = log.ShortMessage.Split("::");

                    return new LogsImportModel
                    {
                        Name = logParts.Length > 2 ? logParts[1].Trim() + " " + logParts[2].Trim() : null,
                        Link = log.Id,
                        LastExecutionDate = logParts.Length > 1 ? logParts[4].Trim() : null
                    };
                })
                .Where(log => log.LastExecutionDate != null)
                .GroupBy(log => log.LastExecutionDate)
                .Select(group => new LogsListByImportDate
                {
                    LastExecutionDate = group.Key,
                    Logs = group.ToList()
                })
                .ToList();

            var ApiLogsList = _logger.GetAllLogs(DateTime.Now.AddMonths(-3), DateTime.Now, "ImportCustomerError", LogLevel.Warning);

            foreach (var importer in importers)
            {
                model.Add(new NetSuiteImporterModel
                {
                    Id = importer.Id,
                    Name = importer.Name,
                    ActionName = importer.ActionName,
                    ControllerName = importer.ControllerName,
                    LastExecutionDate = importer.LastExecutionDate,
                    ActiveAll = _settingService.GetSetting("NetSuiteImporterModel.ActiveAll").Value,
                    ProductLogs = listProductLogs,
                    CustomerLogsIssue = listCustomerLogs,
                    OrderLogs= listOrderLogs,
                    PendingOrdersLogs= listPendingOrderLogs,
                    ApiLogs = _pendingDataToSyncService.GetAllPendingDataToSync()
                });
            }

            return View("~/Plugins/Misc.NetSuiteConnector/Views/Importers.cshtml", model);
        }

        [HttpGet]
        public virtual IActionResult ImportCustomers(string IdCustomer, string Type)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            try
            {
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.CustomerImporter, IdCustomer, Type);

                if (importer != null)
                {
                    importer.LastExecutionDate = DateTime.UtcNow;
                    _importManager.UpdateImporter(importer);
                    _settingService.ClearCache();

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

            return Index();
        }

        [HttpGet]
        public virtual IActionResult ImportProducts()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            try
            {
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.ProductImporter);

                if (importer != null)
                {
                    importer.LastExecutionDate = DateTime.UtcNow;
                    _importManager.UpdateImporter(importer);
                    _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Importer.Products"));
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

            return Index();
        }

        [HttpGet]
        public virtual IActionResult ImportOrders(string IdCustomer, string Type)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            try
            {
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.OrderImporter, IdCustomer, Type);

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

            return Index();
        }

        [HttpGet]
        public virtual IActionResult ImportPendingOrders()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            try
            {
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.PendingOrderImporter);

                if (importer != null)
                {
                    importer.LastExecutionDate = DateTime.UtcNow;
                    _importManager.UpdateImporter(importer);
                    _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Importer.PendingOrder"));
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

            return Index();
        }

        [HttpGet]
        public virtual IActionResult ImportImages()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            try
            {
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.ImageImporter);

                if (importer != null)
                {
                    importer.LastExecutionDate = DateTime.UtcNow;
                    _importManager.UpdateImporter(importer);
                    _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Importer.Images"));
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

            return Index();
        }

        
        [HttpPost]
        public virtual IActionResult ListCustomerImportLogs()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var listCustomerLogs = new List<LogNetsuiteImport>();

            try
            {
                var customerLogs = _logImportNetsuite.GetLogs().Take(3);
                listCustomerLogs = customerLogs
					//.Where(log => log.StartsWith("ImportCustomerError"))
					.Select(log =>
					{
					//var logParts = log.ShortMessage.Split("::");

				        return new LogNetsuiteImport
                        {
						    ImportName = "Test 1"
						 };
			            })
					.ToList();

				//var customerLogs = _logger.GetAllLogs(DateTime.Now.AddMonths(-3), DateTime.Now, "ImportCustomerError", LogLevel.Warning);

				//listCustomerLogs = customerLogs
				//    .Where(log => log.ShortMessage.StartsWith("ImportCustomerError"))
				//    .Select(log =>
				//    {
				//        var logParts = log.ShortMessage.Split("::");

				//        return new LogsImportModel
				//        {
				//            Name = logParts.Length > 2 ? logParts[1].Trim() + " " + logParts[2].Trim() : null,
				//            Link = log.Id,
				//            LastExecutionDate = logParts.Length > 1 ? logParts[4].Trim() : null
				//        };
				//    })
				//    .Where(log => log.LastExecutionDate != null)
				//    .GroupBy(log => log.LastExecutionDate)
				//    .Select(group => new LogsListByImportDate
				//    {
				//        LastExecutionDate = group.Key,
				//        Logs = group.ToList()
				//    })
				//    .ToList();


			}
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
            }

            return Json(listCustomerLogs);
        }

        [HttpPost]
        public virtual IActionResult ListActivityLog(CustomerActivityLogSearchModel searchModel)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageOrders))
                return AccessDeniedDataTablesJson();

            //var listCustomerLogs = new List<CustomerActivityLogListModel>();

            //var customerLogs = _logImportNetsuite.GetLogs().Take(3);

			//foreach (var log in customerLogs)
			//{
   //             var modelT=new CustomerActivityLogModel // Change the type here
   //             {
   //                 ActivityLogTypeName = log.ImportName,
   //                 CreatedOn = log.DateCreate,
   //                 Comment = log.DataFromNetsuite
   //             };

   //             listCustomerLogs.Add(modelT);
   //         }
            //listCustomerLogs = customerLogs.Select(log =>
            //{
            //    return new CustomerActivityLogModel // Change the type here
            //    {
            //        ActivityLogTypeName = log.ImportName,
            //        CreatedOn = log.DateCreate,
            //        Comment = log.DataFromNetsuite
            //    };
            //})
            //.ToList();

            //////try to get a customer with the specified id
            //var customer = _customerService.GetCustomerById(searchModel.CustomerId)
            //    ?? throw new ArgumentException("No customer found with the specified id");

            //////prepare model
           // var model = _customerModelFactory.PrepareCustomerActivityLogListModel(searchModel, customer);
            var model = PrepareCustomerActivityLogListModel(searchModel);

            return Json(model);
        }

        public virtual ImportActivityLogListModel PrepareCustomerActivityLogListModel(CustomerActivityLogSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            //get customer activity log
            var activityLog = _logImportNetsuite.SearchLogs(customerId: 0,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var pageList = activityLog.Select(logItem =>
            {
                //fill in model values from the entity
                var customerActivityLogModel = new LogNetsuiteImportModel();

                //fill in additional values (not existing in the entity)
                customerActivityLogModel.ImportName = logItem.ImportName;
                customerActivityLogModel.DataFromNetsuite = logItem.DataFromNetsuite;
                customerActivityLogModel.DataUpdatedNetsuite = logItem.DataUpdatedNetsuite;
                customerActivityLogModel.RegisterId = logItem.RegisterId;

                //convert dates to the user time
                customerActivityLogModel.DateCreate = logItem.DateCreate;

                return customerActivityLogModel;
            }).ToList().ToPagedList(searchModel);

            //prepare list model
            var model = new ImportActivityLogListModel().PrepareToGrid(searchModel, pageList, () => pageList);
            
            return model;
        }

        [HttpGet]
        public virtual IActionResult ListCustomerImportLogsALl()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var listCustomerLogs = new List<LogsListByImportDate>();

            try
            {
                var customerLogs = _logger.GetAllLogs(DateTime.Now.AddMonths(-3), DateTime.Now, "ImportCustomerError", LogLevel.Warning);

                listCustomerLogs = customerLogs
                    .Where(log => log.ShortMessage.StartsWith("ImportCustomerError"))
                    .Select(log =>
                    {
                        var logParts = log.ShortMessage.Split("::");

                        return new LogsImportModel
                        {
                            Name = logParts.Length > 2 ? logParts[1].Trim() + " " + logParts[2].Trim() : null,
                            Link = log.Id,
                            LastExecutionDate = logParts.Length > 1 ? logParts[4].Trim() : null
                        };
                    })
                    .Where(log => log.LastExecutionDate != null)
                    .GroupBy(log => log.LastExecutionDate)
                    .Select(group => new LogsListByImportDate
                    {
                        LastExecutionDate = group.Key,
                        Logs = group.ToList()
                    })
                    .ToList();


            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc);
            }

            return Json(listCustomerLogs);
        }


        [HttpGet]
        public virtual IActionResult ImportDataFromNetsuite()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            try
            {
                var importer = _importManager.GetImporter(Domain.ImporterIdentifier.ImportDataFromNetsuite);

                if (importer != null)
                {
                    importer.LastExecutionDate = DateTime.UtcNow;
                    _importManager.UpdateImporter(importer);
                    _notificationService.SuccessNotification(_localizationService.GetResource("Plugins.Misc.NetSuiteConnector.Importer.PendingOrder"));
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

            return Index();
        }

        #endregion
    }
}
