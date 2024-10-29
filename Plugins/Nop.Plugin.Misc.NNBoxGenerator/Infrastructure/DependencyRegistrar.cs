using Autofac;
using Autofac.Core;
using Nop.Core.Configuration;
using Nop.Core.Data;
using Nop.Core.Domain.Tax;
using Nop.Core.Infrastructure;
using Nop.Core.Infrastructure.DependencyManagement;
using Nop.Data;
using Nop.Plugin.Misc.NNBoxGenerator.Data;
using Nop.Plugin.Misc.NNBoxGenerator.Domain;
using Nop.Plugin.Misc.NNBoxGenerator.Services;
using Nop.Services.Tax;
using Nop.Web.Framework.Infrastructure.Extensions;


namespace Nop.Plugin.Misc.NNBoxGenerator.Infrastructure
{
    public class DependencyRegistrar : IDependencyRegistrar
    {
        public int Order => 10;

        public void Register(ContainerBuilder builder, ITypeFinder typeFinder, NopConfig config)
        {
            builder.RegisterType<BoxPackingService>().As<IBoxPackingService>().InstancePerLifetimeScope();
            builder.RegisterPluginDataContext<NNBoxGeneratorContext>("nop_object_context_nn_delivery");

            builder.RegisterType<EfRepository<BSBox>>().As<IRepository<BSBox>>()
                .WithParameter(ResolvedParameter.ForNamed<IDbContext>("nop_object_context_nn_delivery"))
                .InstancePerLifetimeScope();
        }
    }
}
