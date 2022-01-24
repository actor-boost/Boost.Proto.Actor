using Boost.Proto.Actor.DependencyInjection;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace Boost.Proto.Actor.BlazorWasm;

public static class DependencyInjectionExtensions
{
    public static WebAssemblyHostBuilder UseProtoActor(this WebAssemblyHostBuilder builder,
                                                       Action<IServiceProvider, ProtoActorWasmOption> config)
    {
        builder.Services.AddProtoActorWasm(config);
        //builder.RootComponents.Add<ProtoActorService>("#app::after");
        return builder;
    }

    internal static IServiceCollection AddProtoActorWasm(this IServiceCollection services,
                                                         Action<IServiceProvider, ProtoActorWasmOption> config)
    {
        services.AddSingleton(sp =>
        {
            var ret = ActivatorUtilities.CreateInstance<ProtoActorWasmOption>(sp);
            config?.Invoke(sp, ret);
            return ret;
        });

        services.AddSingleton(sp => new FuncActorSystem(sp.GetService<ProtoActorWasmOption>()!.FuncActorSystem));
        services.AddSingleton(sp => new FuncActorSystemConfig(sp.GetService<ProtoActorWasmOption>()!.FuncActorSystemConfig));
        services.AddSingleton(sp => new FuncRootContext(sp.GetService<ProtoActorWasmOption>()!.FuncRootContext));
        services.AddSingleton(sp => new FuncActorSystemStart(sp.GetService<ProtoActorWasmOption>()!.FuncActorSystemStart));

        services.AddProtoActor();
        return services;
    }
}
