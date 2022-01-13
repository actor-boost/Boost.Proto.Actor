using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.OpenTelemetry;

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

            services.AddSingleton(sp =>
            {
                var funcRootContext = sp.GetServices<FuncRootContext>()
                                        .Reduce((x, y) => x + y);

                return funcRootContext(sp.GetService<RootContext>().WithTracing());
            });

            services.AddSingleton(new FuncProps(x => x.WithTracing()));
        });

        return host;
    }
}
