using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Shipping.NNDelivery.Data;
using Nop.Plugin.Shipping.NNDelivery.Domain;
using Nop.Plugin.Shipping.NNDelivery.Services;
using Nop.Web.Framework.Infrastructure.Extensions;

namespace Nop.Plugin.Shipping.NNDelivery.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 10;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<DeliveryRoutesService>().As<IDeliveryRoutesService>().InstancePerLifetimeScope();
            builder.RegisterPluginDataContext<NNDeliveryContext>("nop_object_context_nn_delivery");

            builder.RegisterType<EfRepository<DeliveryRoutes>>().As<IRepository<DeliveryRoutes>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_nn_delivery"))
                .InstancePerLifetimeScope();
        }
    }
}