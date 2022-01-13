namespace Boost.Proto.Actor.DependencyInjection;

public interface IPropsFactory<out TActor> where TActor : IActor
{
    Props Create(params object[] args);
}
