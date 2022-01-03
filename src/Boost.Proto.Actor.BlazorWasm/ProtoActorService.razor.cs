using Microsoft.AspNetCore.Components;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm;

internal delegate void ProtoActorServiceStart(IServiceProvider sp, IRootContext root);

public partial class ProtoActorService
{
    [Inject]
    private RootContext RootContext { get; set; }

    [Inject]
    private IServiceProvider ServiceProvider { get; set; }

    [Inject]
    private ProtoActorServiceStart ProtoActorServiceStart { get; set; }

    protected override Task OnInitializedAsync()
    {
        ProtoActorServiceStart(ServiceProvider, RootContext);
        return base.OnInitializedAsync();
    }
}
