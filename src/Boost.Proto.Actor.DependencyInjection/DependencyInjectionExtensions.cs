using Boost.Proto.Actor.DependencyInjection.Implementations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Boost.Proto.Actor.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProtoActor(this IServiceCollection services)
    {
        services.AddSingleton(sp => new FuncProps(Props (Props _) => _));

        services.AddSingleton(sp =>
        {
            Log.SetLoggerFactory(sp.GetRequiredService<ILoggerFactory>());

            var funcConfig = sp.GetServices<FuncActorSystemConfig>()
                               .Reduce((x, y) => x + y);
            var funcSystem = sp.GetServices<FuncActorSystem>()
                               .Reduce((x, y) => x + y);

            return funcSystem(new ActorSystem(funcConfig(ActorSystemConfig.Setup())));
        });

        services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
        services.AddSingleton(sp => sp.GetService<ActorSystem>().Root);
        services.AddSingleton(sp =>
        {
            var funcRootContext = sp.GetServices<FuncRootContext>()
                                    .Reduce((x, y) => x + y);

            return funcRootContext(sp.GetService<RootContext>());
        });
        return services;
    }
}
