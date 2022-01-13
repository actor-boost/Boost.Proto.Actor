using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Boost.Proto.Actor.OpenTelemetry;

public static class HostExtensions
{
    public static IHostBuilder UseProtoActorOpenTelemetry(this IHostBuilder host,
                                                          Action<IServiceProvider, HostOption> config)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton(sp =>
            {
                var ret = ActivatorUtilities.CreateInstance<HostOption>(sp);
                config?.Invoke(sp, ret);
                return ret;
            });
        });

        return host;
    }
}
