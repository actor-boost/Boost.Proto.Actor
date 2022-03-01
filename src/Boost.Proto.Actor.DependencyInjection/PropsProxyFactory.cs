using System.Collections.Generic;

namespace Boost.Proto.Actor.DependencyInjection;

public record PropsProxyFactory<T>(IEnumerable<FuncProps> FuncProps,
                                   Props InjectionProps)
    : IPropsFactory<T> where T : IActor
{
    public Props Create(params object[] args)
    {
        var func = FuncProps.Aggregate((x, y) => z => y(x(z)));
        return func(InjectionProps);
    }
}
