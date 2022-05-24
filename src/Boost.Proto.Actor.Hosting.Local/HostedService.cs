using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local
{
    internal record HostedService(IEnumerable<FuncActorSystemStart> ActorSystemStarts,
                                  IRootContext Root) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ActorSystemStarts.Aggregate((x, y) => z => y(x(z)))(Root);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken) => await Root.System.ShutdownAsync();
    }
}
