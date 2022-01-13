using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local;

public class ProtoActorLocalOption : IFuncActorSystemConfig, IFuncActorSystem, IActorSystemStart
{
    public ProtoActorLocalOption(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; }
        = _ => _;

    public Func<ActorSystem, ActorSystem> FuncSystem { get; set; }
        = _ => _;

    public Action<IRootContext> ActorSystemStart { get; set; }
        = ctx => { };
}
