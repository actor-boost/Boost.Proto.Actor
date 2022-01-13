namespace Boost.Proto.Actor.DependencyInjection;

public interface IFuncActorSystemConfig
{
    Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; }
}
