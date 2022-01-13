namespace Boost.Proto.Actor.Hosting.Local.SampleApp.Actors;

public class CounterActor : IActor
{
    int Count { get; set; } = 0;

    public Task ReceiveAsync(IContext ctx) => ctx.Message switch
    {
        int m => Handle(m, ctx),
        _ => Task.CompletedTask
    };
    private Task Handle(int m, IContext ctx)
    {
        Count += m;
        if (ctx.Sender is not null)
        {
            ctx.Respond(Count);
        }

        return Task.CompletedTask;
    }
}
