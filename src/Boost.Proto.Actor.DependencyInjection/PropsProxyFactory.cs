using System.Collections.Generic;

namespace Boost.Proto.Actor.DependencyInjection;

public record PropsProxyFactory<T>(IEnumerable<FuncProps> FuncProps,
                                   IServiceProvider ServiceProvider,
                                   Props InjectionProps)
    : IPropsFactory<T> where T : IActor
{
    public Props Create(params object[] args)
    {
        var func = FuncProps.Aggregate((x, y) => z => y(x(z)));
        return func(InjectionProps);
    }
}
