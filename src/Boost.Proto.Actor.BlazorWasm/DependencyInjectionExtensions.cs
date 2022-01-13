using Boost.Proto.Actor.BlazorWasm;
using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Local;
using Proto;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProtoActorWasm(this IServiceCollection services,
                                                       Action<IServiceProvider, ProtoActorWasmOption> config)
    {
        services.AddSingleton(sp =>
        {
            var ret = sp.CreateInstance<ProtoActorWasmOption>();
            config?.Invoke(sp, ret);
            return ret;
        });

        services.AddProtoActor();
        return services;
    }
}
