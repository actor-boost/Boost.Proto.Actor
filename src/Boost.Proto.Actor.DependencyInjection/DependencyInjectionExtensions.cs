using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Boost.Proto.Actor.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProtoActor(this IServiceCollection services)
    {
        services.AddSingleton<FuncProps>(sp => _ => _);
        services.AddSingleton<FuncActorSystem>(sp => _ => _);
        services.AddSingleton<FuncActorSystemConfig>(sp => _ => _);
        services.AddSingleton<FuncRootContext>(sp => _ => _);
        services.AddSingleton<FuncIRootContext>(sp => _ => _);

        services.AddSingleton(sp =>
        {
            Log.SetLoggerFactory(sp.GetRequiredService<ILoggerFactory>());

            var funcConfig = sp.GetServices<FuncActorSystemConfig>()
                               .Reduce((x, y) => z => y(x(z)));
            var funcSystem = sp.GetServices<FuncActorSystem>()
                               .Reduce((x, y) => z => y(x(z)));

            return funcSystem(new ActorSystem(funcConfig(ActorSystemConfig.Setup())));
        });

        services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
        services.AddSingleton(sp => sp.GetService<ActorSystem>().Root);
        services.AddSingleton(sp =>
        {
            var funcRootContext = sp.GetServices<FuncRootContext>()
                                    .Reduce((x, y) => z => x(y(z)));

            var funcIRootContext = sp.GetServices<FuncIRootContext>()
                                     .Reduce((x, y) => z => x(y(z)));

            return funcIRootContext(funcRootContext(sp.GetService<RootContext>()));
        });
        return services;
    }
}
