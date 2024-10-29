using Autofac;
using Nop.Core.Configuration;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Services;
using Nop.Services.Orders;

namespace Nop.Plugin.Misc.CreditCardVaultAuthorizeNet.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 10;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<CustomerAuthorizeNet>().As<ICustomerAuthorizeNet>().InstancePerLifetimeScope();
            builder.RegisterType<CustomerAuthorizeNetService>().As<ICustomerAuthorizeNetService>().InstancePerLifetimeScope();
            builder.RegisterType<OverriddenOrderProcessingService>().As<IOrderProcessingService>().InstancePerLifetimeScope();
            builder.RegisterType<CreditCardVaultService>().As<ICreditCardVaultService>().InstancePerLifetimeScope();

        }
    }
}
