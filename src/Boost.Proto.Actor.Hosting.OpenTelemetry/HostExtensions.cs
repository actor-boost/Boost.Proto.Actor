using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Opentelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

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

            services.AddSingleton<FuncRootContext>(x => x.WithTracing());
            services.AddSingleton<FuncProps>(x => x.WithTracing());
        });

        return host;
    }
}
