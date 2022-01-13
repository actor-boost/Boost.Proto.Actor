namespace Boost.Proto.Actor.DependencyInjection;

public interface IFuncActorSystem
{
    Func<ActorSystem, ActorSystem> FuncSystem { get; set; }
}
