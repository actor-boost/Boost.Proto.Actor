using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Boost.Proto.Actor.DependencyInjection;

public record PropsFactory<T>(IEnumerable<FuncProps> FuncProps,
                              IServiceProvider ServiceProvider)
    : IPropsFactory<T> where T : IActor
{
    public Props Create(params object[] args)
    {
        var func = FuncProps.Aggregate((x, y) => z => y(x(z)));
        return func(Props.FromProducer(() => ActivatorUtilities.CreateInstance<T>(ServiceProvider, args)));
    }

}
