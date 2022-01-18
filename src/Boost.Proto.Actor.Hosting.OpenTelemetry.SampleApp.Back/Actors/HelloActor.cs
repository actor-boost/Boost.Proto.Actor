using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Messages;
using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Models;
using Proto;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Back.Actors;

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
        HelloRequest m when ctx.Sender is not null => Handle(m, ctx),
        _ => Task.CompletedTask,
    };

    private Task Handle(HelloRequest m, IContext ctx)
    {
        State += 1;
        ctx.Respond(new HelloResponse(State.ToString(), new Name("World")));
        return Task.CompletedTask;
    }
}
