using Boost.Proto.Actor.DependencyInjection;
using Proto;

namespace Boost.Proto.Actor.Hosting.Cluster;

internal class FuncClusterActorSystemConfig : IFuncActorSystemConfig
{
    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; }
}
