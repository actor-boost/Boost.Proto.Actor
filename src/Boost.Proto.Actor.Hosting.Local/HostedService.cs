using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local
{
    internal record HostedService(IEnumerable<FuncActorSystemStartAsync> FuncActorSystemStartAsyncs,
                                  IOptions<HostOption> HostOption,
                                  IRootContext Root) : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            var f = FuncActorSystemStartAsyncs.Aggregate((x, y) => async (r, n) =>
            {
                await y(r, n);
                await x(r, n);
            });

            await f(Root, r => Task.CompletedTask);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(HostOption.Value.SystemShutdownDelaySec), CancellationToken.None);
            await Root.System.ShutdownAsync();
        }
    }
}
