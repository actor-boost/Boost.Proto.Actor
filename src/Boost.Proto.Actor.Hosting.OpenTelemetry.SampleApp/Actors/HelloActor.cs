using Proto;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Actors;

public class HelloActor : IActor
{
    public HelloActor(ILogger<HelloActor> logger)
    {
        Logger = logger;
    }

    public int State { get; set; }

    public ILogger Logger { get; }

    public Task ReceiveAsync(IContext ctx) => ctx.Message switch
    {
        int m when ctx.Sender is not null => Handle(m, ctx),
        _ => Task.CompletedTask,
    };
    private Task Handle(int m, IContext ctx)
    {
        State += m;
        ctx.Respond(State);
        return Task.CompletedTask;
    }
}
