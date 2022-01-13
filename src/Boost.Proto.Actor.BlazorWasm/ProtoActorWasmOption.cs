using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local
{
    public class ProtoActorWasmOption : IFuncActorSystemConfig
    {
        public ProtoActorWasmOption(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; }
        = _ => _;

        public Func<ActorSystem, ActorSystem> FuncSystem { get; set; }
            = _ => _;

        public Action<IServiceProvider, IRootContext> ActStart { get; set; }
            = (sp, ctx) => { };
    }
}
