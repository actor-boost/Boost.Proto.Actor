using System;
using Proto;

namespace Boost.Proto.Actor.DependencyInjection
{
    internal class PropsFactory<T> : IPropsFactory<T> where T : IActor
    {
        public PropsFactory(IServiceProvider serviceProvider) =>
            ServiceProvider = serviceProvider;

        public IServiceProvider ServiceProvider { get; }

        public Props Create(params object[] args)
            => Props.FromProducer(() => ServiceProvider.CreateInstance<T>(args))
                    .WithContextDecorator(ctx => new LoggerActorContextDecorator(ctx, ServiceProvider));
    }
}
