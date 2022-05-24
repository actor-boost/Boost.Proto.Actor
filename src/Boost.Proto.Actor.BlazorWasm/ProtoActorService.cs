using Boost.Proto.Actor.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm;

public record ProtoActorService(IRootContext RootContext,
                                IEnumerable<FuncActorSystemStart> ActorSystemStarts)
{
    public void Run()
    {
        ActorSystemStarts.Aggregate((x, y) => z => y(x(z)))(RootContext);
    }
}

public static class ProtoActorServiceExtension
{
    public static WebAssemblyHost RunProtoActor (this WebAssemblyHost host)
    {
        ActivatorUtilities.CreateInstance<ProtoActorService>(host.Services).Run();
        return host;
    }
}
