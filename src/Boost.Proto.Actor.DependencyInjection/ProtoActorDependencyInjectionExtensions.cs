using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.DependencyInjection;

public static class ProtoActorDependencyInjectionExtensions
{
    public static IServiceCollection AddProtoActor(this IServiceCollection services,
                                                   Func<ActorSystemConfig, ActorSystemConfig> configFunc,
                                                   Func<ActorSystem, ActorSystem> sysFunc)
    {
        services.AddSingleton(sp => sysFunc(new ActorSystem(configFunc?.Invoke(ActorSystemConfig.Setup()))));
        services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
        services.AddSingleton<IRootContext>(sp => new RootContext(sp.GetService<ActorSystem>()));
        return services;
    }

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

    public static T CreateInstance<T>(this IServiceProvider sp, params object[] args)
        => ActivatorUtilities.CreateInstance<T>(sp, args);
}
