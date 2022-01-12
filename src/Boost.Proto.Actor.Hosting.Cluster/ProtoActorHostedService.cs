using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;

namespace Boost.Proto.Actor.Hosting.Cluster
{
    public delegate void ProtoActorHostedServiceStart(IServiceProvider serviceProvider, IRootContext root);

    internal record ProtoActorHostedService(IServiceProvider ServiceProvider,
                                            IRootContext Root,
                                            ProtoActorHostedServiceStart ProtoActorHostedServiceStart) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ProtoActorHostedServiceStart?.Invoke(ServiceProvider, Root);
            await Root.System.Cluster().StartMemberAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken) 
        {
            await Root.System.Cluster().ShutdownAsync();
            await Root.System.ShutdownAsync();
        }
    }
}
