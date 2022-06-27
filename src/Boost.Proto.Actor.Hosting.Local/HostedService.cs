using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local
{
    internal record HostedService(IEnumerable<FuncActorSystemStart> ActorSystemStarts,
                                  IOptions<HostOption> HostOption,
                                  IRootContext Root) : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            ActorSystemStarts.Aggregate((x, y) => z => y(x(z)))(Root);
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(TimeSpan.FromSeconds(HostOption.Value.SystemShutdownDelaySec), CancellationToken.None);
            await Root.System.ShutdownAsync();
        }
    }
}
