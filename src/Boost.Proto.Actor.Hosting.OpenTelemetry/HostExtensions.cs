using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Opentelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry;

public static class HostExtensions
{
    public static IHostBuilder UseProtoActorOpenTelemetry(this IHostBuilder host,
                                                          Action<Options, IServiceProvider>? option = null,
                                                          string optionPath = "Boost:Actor:OpenTelemetry")
    {
        host.ConfigureServices((context, services) =>
        {
            option ??= (o, sp) => { };

            services.AddOptions<Options>()
                    .BindConfiguration(optionPath)
                    .PostConfigure(option);

            services.AddSingleton<FuncIRootContext>(x => x.WithTracing());
            services.AddSingleton<FuncRootContext>(x => x.WithTracing());
            services.AddSingleton<FuncProps>(x => x.WithTracing());
        });

        return host;
    }
}
