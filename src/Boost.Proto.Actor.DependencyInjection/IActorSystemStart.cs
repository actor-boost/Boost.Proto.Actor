namespace Boost.Proto.Actor.DependencyInjection;

public interface IActorSystemStart
{
    Action<IServiceProvider, IRootContext> ActorSystemStart { get; set; }
}
