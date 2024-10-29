using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.NetSuiteConnector.Data;
using Nop.Plugin.Misc.NetSuiteConnector.Domain;
using Nop.Plugin.Misc.NetSuiteConnector.Services;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Common;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Custom;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Media;
using Nop.Plugin.Misc.NetSuiteConnector.Services.Order;
using Nop.Services.AccountCredit;
using Nop.Services.Catalog;
using Nop.Services.NN;
using Nop.Services.Tax;
using Nop.Web.Framework.Infrastructure.Extensions;


namespace Nop.Plugin.Misc.NetSuiteConnector.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 10;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<OrderService>().As<IOrderService>().InstancePerLifetimeScope();
            builder.RegisterType<Nop.Services.Catalog.ProductService>().As<Nop.Services.Catalog.IProductService>().InstancePerLifetimeScope();
            builder.RegisterType<Nop.Plugin.Misc.NetSuiteConnector.Services.ProductService>().As<Nop.Plugin.Misc.NetSuiteConnector.Services.IProductService>().InstancePerLifetimeScope();

            //builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<ConnectionService>().As<IConnectionServices>().InstancePerLifetimeScope();


            builder.RegisterType<CustomerService>().As<ICustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<Nop.Services.Catalog.PriceLevelService>().As<Nop.Services.Catalog.IPriceLevelService>().InstancePerLifetimeScope();

            builder.RegisterType<CreditMemoService>().As<ICreditMemoService>().InstancePerLifetimeScope();
            builder.RegisterType<InvoiceService>().As<IInvoiceService>().InstancePerLifetimeScope();
            builder.RegisterType<PaymentService>().As<IPaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<ProductPricesCustomerService>().As<IProductPricesCustomerService>().InstancePerLifetimeScope();
            builder.RegisterType<Nop.Services.Customers.CompanyService>().As< Nop.Services.Customers.ICompanyService> ().InstancePerLifetimeScope();
            builder.RegisterType<ImportManagerService>().As<IImportManagerService>().InstancePerLifetimeScope();
            builder.RegisterType<OAuthBaseHelper>().As<IOAuthBaseHelper>().InstancePerLifetimeScope();
            builder.RegisterType<S3Service>().As<IS3Service>().InstancePerLifetimeScope();
            builder.RegisterType<PendingOrdersToSyncService>().As<IPendingOrdersToSyncService>().InstancePerLifetimeScope();
            builder.RegisterType<PendingOrdersToSyncService>().As<IPendingOrdersToSyncService>().InstancePerLifetimeScope();
            builder.RegisterType<Nop.Services.Invoices.CustomerAccountCreditBalanceService>().As<ICustomerCreditBalanceService>().InstancePerLifetimeScope();
            builder.RegisterType<Nop.Services.Invoices.InvoicePaymentService>().As<Nop.Services.Invoices.IInvoicePaymentService>().InstancePerLifetimeScope();
            builder.RegisterType<CreditManageService>().As<ICreditManageService>().InstancePerLifetimeScope();

            builder.RegisterPluginDataContext<NetSuiteConnectorContext>("nop_object_context_netsuite_connector");

            builder.RegisterType<EfRepository<CreditMemo>>().As<IRepository<CreditMemo>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_netsuite_connector"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<Payment>>().As<IRepository<Payment>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_netsuite_connector"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<ProductPricesCustomer>>().As<IRepository<ProductPricesCustomer>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_netsuite_connector"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<NetSuiteImporter>>().As<IRepository<NetSuiteImporter>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_netsuite_connector"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PendingOrdersToSync>>().As<IRepository<PendingOrdersToSync>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_netsuite_connector"))
                .InstancePerLifetimeScope();

            builder.RegisterType<EfRepository<PriceLevel>>().As<IRepository<PriceLevel>>()
               .InstancePerLifetimeScope();
        }
    }
}
