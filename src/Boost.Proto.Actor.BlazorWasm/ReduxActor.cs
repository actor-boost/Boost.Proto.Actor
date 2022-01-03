using LanguageExt;
using Proto;
using static LanguageExt.Prelude;

namespace Boost.Proto.Actor.BlazorWasm;

public class ReduxActor<TState>
{
    protected ReduxActor(TState state) => State = state;

    private TState State { get; set; }

    protected Task ChangeStateAsync(IContext c, Func<TState, TState> func)
    {
        State = func(State);
        if (State != null)
        {
            c.System.EventStream.Publish(State);
        }
        return Task.CompletedTask;
    }

    protected async Task ChangeStateAsync(IContext c, Func<TState, Task<TState>> func)
    {
        State = await func(State);
        if (State != null)
        {
            c.System.EventStream.Publish(State);
        }
    }

    protected Aff<Unit> ChangeStateAff(IContext c, Func<TState, Aff<TState>> funcAff) =>
        from a in unitAff
        from b in funcAff(State)
        from e in Eff(() =>
        {
            State = b;
            if (b is not null)
            {
                c.System.EventStream.Publish(b);
            }
            return unit;
        })
        select a;
}
