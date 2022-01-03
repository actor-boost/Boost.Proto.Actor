using Boost.Proto.Actor.BlazorWasm;
using Boost.Proto.Actor.DependencyInjection;
using Proto;

namespace Microsoft.Extensions.DependencyInjection;

public static class ProtoActorDependencyInjectionWasmExtensions
{
    public static IServiceCollection AddProtoActorWasm(this IServiceCollection services,
                                                       Func<ActorSystemConfig, ActorSystemConfig> configFunc,
                                                       Func<ActorSystem, ActorSystem> sysFunc,
                                                       Action<IServiceProvider, IRootContext> startAction)
    {
        var protoActorServiceStart = new ProtoActorServiceStart(startAction);
        services.AddProtoActor(configFunc, sysFunc);
        services.AddSingleton(protoActorServiceStart);
        return services;
    }
}
