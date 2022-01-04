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
                                             Action<IServiceProvider, IRootContext> startAction)
    {
        var protoActorServiceStart = new ProtoActorHostedServiceStart(startAction);
        host.ConfigureServices((context, services) =>
        {
            services.AddProtoActor(configFunc, sysFunc);
            services.AddSingleton(protoActorServiceStart);
            services.AddHostedService<ProtoActorHostedService>();
        });

        return host;
    }
}
