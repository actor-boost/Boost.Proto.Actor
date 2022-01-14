using Boost.Proto.Actor.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm;

public partial class ProtoActorService
{
    [Inject]
    public IRootContext RootContext { get; set; }

    [Inject]
    public IEnumerable<FuncActorSystemStart> ActorSystemStarts { get; set; }

    protected override Task OnInitializedAsync()
    {
        ActorSystemStarts.Reduce((x, y) => z => y(x(z)))(RootContext);
        return base.OnInitializedAsync();
    }
}
