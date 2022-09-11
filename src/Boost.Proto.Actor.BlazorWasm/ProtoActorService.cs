using Boost.Proto.Actor.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm;

public record ProtoActorService(IRootContext RootContext,
                                IEnumerable<FuncActorSystemStartAsync> FuncActorSystemStartAsyncs)
{
    public async Task RunAsync()
    {
        var f = FuncActorSystemStartAsyncs.Aggregate((x, y) => async (r, n) =>
        {
            await y(r, n);
            await x(r, n);
        });

        await f(RootContext, r => Task.CompletedTask);
    }
}

public static class ProtoActorServiceExtension
{
    public static async Task RunProtoActorAsync (this WebAssemblyHost host)
    {
        await ActivatorUtilities.CreateInstance<ProtoActorService>(host.Services).RunAsync();
    }
}
