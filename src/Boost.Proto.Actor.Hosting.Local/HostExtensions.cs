using Boost.Proto.Actor.DependencyInjection;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local;

public static class HostExtensions
{
    public static IHostBuilder UseProtoActor(this IHostBuilder host,
                                             Action<HostOption, IServiceProvider>? option = null,
                                             string optionPath = "Boost:Actor:Local")
    {
        option ??= (o, sp) => { };
        Action<HostOption, IServiceProvider> optionPost = (o, sp) => option(o, sp);

        host.ConfigureServices((context, services) =>
        {
            services.AddOptions<HostOption>()
                    .BindConfiguration(optionPath)
                    .PostConfigure(optionPost);

            services.AddSingleton(sp => new FuncIRootContext(sp.GetRequiredService<IOptions<HostOption>>().Value.FuncIRootContext));
            services.AddSingleton(sp => new FuncActorSystem(sp.GetRequiredService<IOptions<HostOption>>().Value.FuncActorSystem));
            services.AddSingleton(sp => new FuncActorSystemConfig(sp.GetRequiredService<IOptions<HostOption>>().Value.FuncActorSystemConfig));
            services.AddSingleton(sp => new FuncActorSystemStart(sp.GetRequiredService<IOptions<HostOption>>().Value.FuncActorSystemStart));
            
            services.AddHostedService<HostedService>();
            services.AddProtoActor();
        });

        return host;
    }
}
