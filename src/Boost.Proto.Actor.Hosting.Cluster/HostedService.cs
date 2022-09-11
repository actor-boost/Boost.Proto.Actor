using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Proto;
using Proto.Cluster;

namespace Boost.Proto.Actor.Hosting.Cluster
{
    internal record HostedService(IEnumerable<FuncActorSystemStartAsync> FuncActorSystemStartAsyncs,
                                  IOptions<Options> HostOption,
                                  IRootContext Root) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var f = FuncActorSystemStartAsyncs.Aggregate((x, y) => async (r, n) =>
            {
                await y(r, n);
                await x(r, n);
            });

            await f(Root, async r => await r.System.Cluster().StartMemberAsync());
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(HostOption.Value.SystemShutdownDelaySec), CancellationToken.None);
            await Root.System.Cluster().ShutdownAsync();
            await Root.System.ShutdownAsync();
        }
    }
}
