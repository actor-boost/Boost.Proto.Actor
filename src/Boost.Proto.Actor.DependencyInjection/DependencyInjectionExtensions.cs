using System;
using Microsoft.Extensions.DependencyInjection;
using Proto;

namespace Boost.Proto.Actor.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static ActorSystem WithServiceProvider(this ActorSystem actorSystem, IServiceProvider serviceProvider)
    {
        var diExtension = new ServiceProviderExtension(serviceProvider);
        actorSystem.Extensions.Register(diExtension);
        return actorSystem;
    }

    public static IPropsFactory<T> PropsFactory<T>(this ActorSystem actorSystem) where T : IActor
        => actorSystem.ServiceProvider().GetRequiredService<IPropsFactory<T>>();

    public static IPropsFactory<T> PropsFactory<T>(this IRootContext context) where T : IActor
        => context.System.ServiceProvider().GetRequiredService<IPropsFactory<T>>();

    public static IPropsFactory<T> PropsFactory<T>(this IContext context) where T : IActor
        => context.System.ServiceProvider().GetRequiredService<IPropsFactory<T>>();

    public static IServiceProvider ServiceProvider(this ActorSystem system) => system.Extensions.Get<ServiceProviderExtension>()!.ServiceProvider;


}
