using Proto;

namespace Boost.Proto.Actor.Hosting.Cluster.SampleApp.Actors;

public class HelloActor : IActor
{
    public HelloActor(ILogger<HelloActor> logger)
    {
        Logger = logger;
    }

    public ILogger Logger { get; }

    public Task ReceiveAsync(IContext ctx) => ctx.Message switch
    {
        var m when ctx.Sender is not null => Handle(m, ctx),
        _ => Task.CompletedTask,
    };
    private Task Handle(object? m, IContext ctx)
    {
        ctx.Respond(m!);
        return Task.CompletedTask;
    }
}
