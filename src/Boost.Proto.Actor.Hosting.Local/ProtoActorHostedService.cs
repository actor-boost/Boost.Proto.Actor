using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local
{
    public delegate void ProtoActorHostedServiceStart(IServiceProvider serviceProvider, IRootContext root);

    internal record ProtoActorHostedService(IServiceProvider ServiceProvider,
                                            IRootContext Root,
                                            ProtoActorHostedServiceStart ProtoActorHostedServiceStart) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ProtoActorHostedServiceStart?.Invoke(ServiceProvider, Root);
            await Task.Delay(300);
        }

        public async Task StopAsync(CancellationToken cancellationToken) => await Root.System.ShutdownAsync();
    }
}
