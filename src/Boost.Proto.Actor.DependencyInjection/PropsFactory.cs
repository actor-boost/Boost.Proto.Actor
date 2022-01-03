using System;
using Proto;

namespace Boost.Proto.Actor.DependencyInjection
{
    internal class PropsFactory<T> : IPropsFactory<T> where T : IActor
    {
        public PropsFactory(IRootContext rootContext, IServiceProvider serviceProvider)
        {
            RootContext = rootContext;
            ServiceProvider = serviceProvider;
        }

        public IRootContext RootContext { get; }
        public IServiceProvider ServiceProvider { get; }

        public Props Create(params object[] args)
            => Props.FromProducer(() => ServiceProvider.CreateInstance<T>(args))
                    .WithContextDecorator(ctx => new LoggerActorContextDecorator(ctx, ServiceProvider));
    }
}
