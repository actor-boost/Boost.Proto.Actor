using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Boost.Proto.Actor.BlazorWasm")]
[assembly: InternalsVisibleTo("Boost.Proto.Actor.Hosting.Local")]
[assembly: InternalsVisibleTo("Boost.Proto.Actor.Hosting.Cluster")]

namespace Boost.Proto.Actor.DependencyInjection;

internal static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProtoActor(this IServiceCollection services)
    {
        services.AddSingleton(sp =>
        {
            var configFunc = (from x in sp.GetServices<IFuncActorSystemConfig>()
                              select x.FuncActorSystemConfig).Reduce((x, y) => x + y);
            var funcSystem = (from x in sp.GetServices<IFuncActorSystem>()
                              select x.FuncSystem).Reduce((x, y) => x + y);

            return funcSystem(new ActorSystem(configFunc?.Invoke(ActorSystemConfig.Setup())));
        });
        services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
        services.AddSingleton<IRootContext>(sp => sp.GetService<ActorSystem>().Root);
        return services;
    }

    public static T CreateInstance<T>(this IServiceProvider sp, params object[] args)
        => ActivatorUtilities.CreateInstance<T>(sp, args);
}
