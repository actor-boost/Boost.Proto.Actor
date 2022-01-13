using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local
{
    internal record ProtoActorHostedService(IActorSystemStart ActorSystemStart,
                                            IRootContext Root) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ActorSystemStart.ActorSystemStart?.Invoke(Root);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken) => await Root.System.ShutdownAsync();
    }
}
