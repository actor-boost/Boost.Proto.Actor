using Microsoft.AspNetCore.Components;
using Boost.Proto.Actor.BlazorWasm.SampleApp.Actors.Counter;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm.SampleApp.Pages;

public partial class Counter
{
    [Inject]
    private IRootContext Context { get; set; }

    private Action<object> Command { get; set; }

    private CounterState State { get; set; }

    protected override void OnInitialized()
    {
        Command = c => Context.Send(new PID(Context.System.Address, nameof(CounterActor)), c);
        Command(new ViewInitialized());
        var a = Context.System.EventStream.Subscribe<CounterState>(s =>
        {
            State = s;
            StateHasChanged();
        });
    }
}
