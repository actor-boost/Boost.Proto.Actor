using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using Proto;
using static LanguageExt.Prelude;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.Tests;

public record Hi();

public enum HelloActorState
{
    Initialize,
    Ready,
    End
}

public record HelloActor(PID WorldActor) : IActor
{
    HelloActorState HelloActorState { get; set; } = HelloActorState.Initialize;

    private Dictionary<HelloActorState, Func<IContext, Aff<HelloActorState>>> Machine = new()
    {
        [HelloActorState.Initialize] = (IContext c) => Eff(() =>
        {
            return HelloActorState.Ready;
        }),

        [HelloActorState.Ready] = c => c.Message switch
        {
            Hi => from __ in unitAff
                  from _1 in c.RequestAsync<Unit>(WorldActor, new Hi()).ToAff()
                  from _2 in Eff(() =>
                  {
                      c.Respond(_1);
                      return unit;
                  })
                  select HelloActorState.End,
            _ => Eff(() => HelloActorState.Ready)
        },
    };

    public async Task ReceiveAsync(IContext context)
    {
        var q = Machine[HelloActorState].Invoke(context);
        var ret = await q.Run();
        HelloActorState = ret.ThrowIfFail();
    }
}

