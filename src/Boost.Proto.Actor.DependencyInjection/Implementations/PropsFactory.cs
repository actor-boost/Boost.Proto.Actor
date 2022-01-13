using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Boost.Proto.Actor.DependencyInjection.Implementations;

public record PropsFactory<T>(IEnumerable<FuncProps> FuncProps,
                              IServiceProvider ServiceProvider)
    : IPropsFactory<T> where T : IActor
{
    public Props Create(params object[] args)
        => FuncProps.Reduce((x, y) => x + y)(Props.FromProducer(() => ActivatorUtilities.CreateInstance<T>(ServiceProvider, args)));
}

