using System;
using Boost.Proto.Actor.Decorators;
using Proto;
using static LanguageExt.Prelude;

namespace Boost.Proto.Actor.DependencyInjection;

public record PropsFactory<T>(IServiceProvider ServiceProvider,
                              bool UseLoggerDecorator = true) : IPropsFactory<T> where T : IActor
{
    public Props Create(params object[] args) =>
        match(from a in Some(Props.FromProducer(() => ServiceProvider.CreateInstance<T>(args)))
              from b in Some(UseLoggerDecorator ? a.WithContextDecorator(ctx => ServiceProvider.CreateInstance<LoggerActorContextDecorator>(ctx)) : a)
              select b,
              x => x,
              e => null);
}
