using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.NN;
using Nop.Core.Domain.Orders;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.Order;
using Nop.Plugin.Misc.NetSuiteConnector.DTOs.SalesOrderItem;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.NetSuiteConnector.Services
{
    public partial interface IOrderService
    {
        #region Orders

        void ImportOrders(string date);
        void ImportOrders(string LastExecutionDate, string idorder = null, string type = null);
        void ImportOrders(string LastExecutionDate, string idorder = null, string type = null, List<PendingDataToSync> listcustomers = null);


        void SyncPendingOrders(string date=null);

        void CreateOrderInNetsuite(string LastExecutionDateGeneral, int orderId);

        void SendOrderNetsuite(Nop.Core.Domain.Orders.Order order);
        void SendOrderNetsuiteEventCreateOrder(Nop.Core.Domain.Orders.Order order);
        void GetTransactionInfoOpenAndPending(CompanyCustomerMapping comp, Customer customer, string LastExecutionDate = null);

        void GetOpenInvoiceByCompanyId(int companyNetsuiteId, int companyId);
        #endregion
    }
}
