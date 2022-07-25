using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Proto;
using Proto.Cluster;

namespace Boost.Proto.Actor.Hosting.Cluster
{
    internal record HostedService(IEnumerable<FuncActorSystemStart> FuncActorSystemStarts,
                                  IOptions<Options> HostOption,
                                  IRootContext Root) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            FuncActorSystemStarts.Aggregate((x, y) => z => y(x(z)))(Root);
            await Root.System.Cluster().StartMemberAsync();
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(HostOption.Value.SystemShutdownDelaySec), CancellationToken.None);
            await Root.System.Cluster().ShutdownAsync();
            await Root.System.ShutdownAsync();
        }
    }
}
