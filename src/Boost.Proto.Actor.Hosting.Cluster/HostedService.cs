using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;

namespace Boost.Proto.Actor.Hosting.Cluster
{
    internal record HostedService(IEnumerable<FuncActorSystemStart> FuncActorSystemStarts,
                                  IRootContext Root) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            FuncActorSystemStarts.Reduce((x, y) => x + y)(Root);
            await Root.System.Cluster().StartMemberAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Root.System.Cluster().ShutdownAsync();
            await Root.System.ShutdownAsync();
        }
    }
}
