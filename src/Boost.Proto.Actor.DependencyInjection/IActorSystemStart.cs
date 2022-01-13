namespace Boost.Proto.Actor.DependencyInjection;

public interface IActorSystemStart
{
    Action<IRootContext> ActorSystemStart { get; set; }
}
