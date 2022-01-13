using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local;

public static class HostExtensions
{
    public static IHostBuilder UseProtoActor(this IHostBuilder host,
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

            services.AddSingleton(sp => new FuncRootContext(sp.GetService<HostOption>()!.FuncRootContext));
            services.AddSingleton(sp => new FuncActorSystem(sp.GetService<HostOption>()!.FuncActorSystem));
            services.AddSingleton(sp => new FuncActorSystemConfig(sp.GetService<HostOption>()!.FuncActorSystemConfig));
            services.AddSingleton(sp => new FuncActorSystemStart(sp.GetService<HostOption>()!.FuncActorSystemStart));
            
            services.AddHostedService<HostedService>();
            services.AddProtoActor();
        });

        return host;
    }
}
