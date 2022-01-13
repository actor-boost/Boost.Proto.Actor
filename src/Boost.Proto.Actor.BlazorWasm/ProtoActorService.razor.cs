using Boost.Proto.Actor.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm;

internal delegate void ProtoActorServiceStart(IServiceProvider sp, IRootContext root);

public partial class ProtoActorService
{
    [Inject]
    private IRootContext RootContext { get; set; }

    [Inject]
    private IActorSystemStart ActorSystemStart { get; set; }

    protected override Task OnInitializedAsync()
    {
        ActorSystemStart.ActorSystemStart?.Invoke(RootContext);
        return base.OnInitializedAsync();
    }
}
