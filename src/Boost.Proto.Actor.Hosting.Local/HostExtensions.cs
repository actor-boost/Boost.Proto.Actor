using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local;

public static class HostExtensions
{
    public static IHostBuilder UseProtoActor(this IHostBuilder host,
                                             Action<IServiceProvider, ProtoActorLocalOption> config)
    {

        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton(sp =>
            {
                var ret = sp.CreateInstance<ProtoActorLocalOption>();
                config?.Invoke(sp, ret);
                return ret;
            });

            services.AddSingleton<IFuncActorSystem>(sp => sp.GetService<ProtoActorLocalOption>());
            services.AddSingleton<IFuncActorSystemConfig>(sp => sp.GetService<ProtoActorLocalOption>());
            services.AddSingleton<IActorSystemStart>(sp => sp.GetService<ProtoActorLocalOption>());

            services.AddProtoActor();
            services.AddSingleton(sp =>
                new ProtoActorHostedServiceStart(sp.GetService<ProtoActorLocalOption>().ActorSystemStart));
            services.AddHostedService<ProtoActorHostedService>();
        });

        return host;
    }
}
