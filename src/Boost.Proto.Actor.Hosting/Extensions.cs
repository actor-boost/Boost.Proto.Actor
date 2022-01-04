using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.Hosting;

public static class Extensions
{
    public static IHostBuilder UseProtoActor(this IHostBuilder host,
                                             Func<ActorSystemConfig, ActorSystemConfig> configFunc,
                                             Func<ActorSystem, ActorSystem> sysFunc,
                                             ProtoActorHostedServiceStart akkaHostedServiceStart)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddProtoActor(configFunc, sysFunc);
            services.AddSingleton(akkaHostedServiceStart);
            services.AddHostedService<ProtoActorHostedService>();
        });

        return host;
    }
}
