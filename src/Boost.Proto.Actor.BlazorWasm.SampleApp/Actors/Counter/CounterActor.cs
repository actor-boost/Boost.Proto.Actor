using Boost.Proto.Actor.BlazorWasm;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm.SampleApp.Actors.Counter;

public class CounterActor : ReduxActor<CounterState>, IActor
{
    public CounterActor(CounterState s) : base(s)
    {
    }

    public Task ReceiveAsync(IContext c) => c.Message switch
    {
        ViewInitialized => ChangeStateAsync(c, s => s),
        Increase => ChangeStateAsync(c, s => s with
        {
            Count = s.Count + 1
        }),
        _ => Task.CompletedTask
    };
}
