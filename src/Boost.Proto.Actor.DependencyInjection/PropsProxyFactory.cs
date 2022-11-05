using System.Collections.Generic;

namespace Boost.Proto.Actor.DependencyInjection;

public record PropsProxyFactory<T>(Props InjectionProps)
    : IPropsFactory<T> where T : IActor
{
    public Props Create(params object[] args) => InjectionProps;
}
