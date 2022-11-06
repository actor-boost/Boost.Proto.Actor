using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto.DependencyInjection;
using Proto.Extensions;

namespace Boost.Proto.Actor.DependencyInjection;

public class MSDIExtension : IActorSystemExtension<MSDIExtension>
{
    public MSDIExtension(IServiceProvider resolver)
    {
        Resolver = resolver;
    }

    public IServiceProvider Resolver { get; }
}

public static class Extensions
{
    public static ActorSystem WithMicrosoftExtensionServiceProvider(this ActorSystem actorSystem, IServiceProvider serviceProvider)
    {
        var diExtension = new MSDIExtension(serviceProvider);
        actorSystem.Extensions.Register(diExtension);

        return actorSystem;
    }

    public static IServiceProvider ServiceProvider(this ActorSystem system) =>
        system.Extensions.GetRequired<MSDIExtension>().Resolver;

    public static IServiceCollection AddProtoActor(this IServiceCollection services)
    {
        services.AddSingleton<FuncProps>(sp => _ => _);
        services.AddSingleton<FuncActorSystem>(sp => _ => _); 
        services.AddSingleton<FuncActorSystemConfig>(sp => _ => _);
        services.AddSingleton<FuncIRootContext>(sp => _ => _);

        services.AddSingleton(sp =>
        {
            Log.SetLoggerFactory(sp.GetRequiredService<ILoggerFactory>());

            var funcConfig = sp.GetServices<FuncActorSystemConfig>()
                               .Aggregate((x, y) => z => y(x(z)));
            var funcSystem = sp.GetServices<FuncActorSystem>()
                               .Aggregate((x, y) => z => y(x(z)));

            return funcSystem(new ActorSystem(funcConfig(ActorSystemConfig.Setup()))
                .WithServiceProvider(sp)
                .WithMicrosoftExtensionServiceProvider(sp));
        });

        services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
        services.AddSingleton(sp =>
        {
            var funcIRootContext = sp.GetServices<FuncIRootContext>()
                                     .Aggregate((x, y) => z => x(y(z)));

            return funcIRootContext(sp.GetRequiredService<ActorSystem>().Root);
        });
        return services;
    }
}
