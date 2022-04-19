using System.Threading.Tasks;
using Proto;
using static LanguageExt.Prelude;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.Tests;

public record WorldActor : IActor
{
    public Task ReceiveAsync(IContext context) => context.Message switch
    {
        Hi => Task.Run(() => context.Respond(unit)),
        _ => Task.CompletedTask
    };
}

