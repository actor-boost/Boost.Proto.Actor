using Microsoft.Extensions.DependencyInjection;

namespace Boost.Proto.Actor.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProtoActor(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var configFunc = sp.GetService<IFuncActorSystemConfig>().FuncActorSystemConfig;
            var funcSystem = sp.GetService<IFuncActorSystem>().FuncSystem;

            return funcSystem(new ActorSystem(configFunc?.Invoke(ActorSystemConfig.Setup())));
        });
        services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
        services.AddSingleton<IRootContext>(sp => sp.GetService<ActorSystem>().Root);
        return services;
    }

    public static T CreateInstance<T>(this IServiceProvider sp, params object[] args)
        => ActivatorUtilities.CreateInstance<T>(sp, args);
}
