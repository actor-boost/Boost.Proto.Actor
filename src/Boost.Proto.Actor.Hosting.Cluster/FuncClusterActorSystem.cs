using Boost.Proto.Actor.DependencyInjection;
using Proto;

namespace Boost.Proto.Actor.Hosting.Cluster;

internal class FuncClusterActorSystem : IFuncActorSystem
{
    public Func<ActorSystem, ActorSystem> FuncSystem { get; set; }
}
