using System;
using Microsoft.Extensions.DependencyInjection;
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
    public static T CreateInstance<T>(this IServiceProvider sp, params object[] args)
        => ActivatorUtilities.CreateInstance<T>(sp, args);
}
