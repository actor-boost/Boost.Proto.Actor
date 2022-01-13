using Boost.Proto.Actor.BlazorWasm;
using Boost.Proto.Actor.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static WebAssemblyHostBuilder UseProtoActor(this WebAssemblyHostBuilder builder,
                                                       Action<IServiceProvider, ProtoActorWasmOption> config)
    {
        builder.Services.AddProtoActorWasm(config);
        builder.RootComponents.Add<ProtoActorService>("#app::after");
        return builder;
    }

    internal static IServiceCollection AddProtoActorWasm(this IServiceCollection services,
                                                         Action<IServiceProvider, ProtoActorWasmOption> config)
    {
        services.AddSingleton(sp =>
        {
            var ret = sp.CreateInstance<ProtoActorWasmOption>();
            config?.Invoke(sp, ret);
            return ret;
        });

        services.AddSingleton<IFuncActorSystem>(sp => sp.GetService<ProtoActorWasmOption>());
        services.AddSingleton<IFuncActorSystemConfig>(sp => sp.GetService<ProtoActorWasmOption>());
        services.AddSingleton<IActorSystemStart>(sp => sp.GetService<ProtoActorWasmOption>());

        services.AddProtoActor();
        return services;
    }
}
