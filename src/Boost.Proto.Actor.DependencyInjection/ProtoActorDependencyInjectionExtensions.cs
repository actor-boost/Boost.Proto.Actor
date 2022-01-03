using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;

namespace Boost.Proto.Actor.DependencyInjection;

public static class ProtoActorDependencyInjectionExtensions
{
    public static IHostBuilder UseProtoActor(this IHostBuilder host,
                                             Func<ActorSystemConfig, ActorSystemConfig> configFunc,
                                             Func<ActorSystem, ActorSystem> sysFunc,
                                             ProtoActorHostedServiceStart akkaHostedServiceStart)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton(akkaHostedServiceStart);
            services.AddSingleton(sp => sysFunc(new ActorSystem(configFunc?.Invoke(ActorSystemConfig.Setup()))
                                                                           .WithServiceProvider(sp)));
            services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
            services.AddHostedService<ProtoActorHostedService>();
            services.AddSingleton(sp => (IRootContext)new RootContext(sp.GetService<ActorSystem>()));
        });

        return host;
    }

    public static T CreateInstance<T>(this IServiceProvider sp, params object[] args)
        => ActivatorUtilities.CreateInstance<T>(sp, args);
}
