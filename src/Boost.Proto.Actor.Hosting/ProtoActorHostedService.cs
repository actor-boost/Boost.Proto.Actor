using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting
{
    public delegate void ProtoActorHostedServiceStart(IServiceProvider serviceProvider, IRootContext root);

    internal class ProtoActorHostedService : IHostedService
    {
        public ProtoActorHostedService(IServiceProvider serviceProvider,
                                       IRootContext root,
                                       ProtoActorHostedServiceStart protoActorHostedServiceStart)
        {
            ServiceProvider = serviceProvider;
            Root = root;
            ProtoActorHostedServiceStart = protoActorHostedServiceStart;
        }

        public IServiceProvider ServiceProvider { get; }
        public IRootContext Root { get; }
        public ProtoActorHostedServiceStart ProtoActorHostedServiceStart { get; }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            ProtoActorHostedServiceStart?.Invoke(ServiceProvider, Root);

            await Task.Delay(300);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Root.System.ShutdownAsync();
        }
    }
}
