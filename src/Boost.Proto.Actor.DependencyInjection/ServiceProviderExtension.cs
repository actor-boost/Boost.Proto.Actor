using System;
using Proto.Extensions;

namespace Boost.Proto.Actor.DependencyInjection
{
    public class ServiceProviderExtension : IActorSystemExtension<ServiceProviderExtension>
    {
        public ServiceProviderExtension(IServiceProvider serviceProvider)
            => ServiceProvider = serviceProvider;

        public IServiceProvider ServiceProvider { get; }
    }
}
