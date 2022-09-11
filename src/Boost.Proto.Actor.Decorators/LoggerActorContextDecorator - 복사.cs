using Boost.Proto.Actor.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using ReceiveTimeoutMessage = Proto.ReceiveTimeout;

namespace Boost.Proto.Actor.Decorators;

public class AutoDownDecorator : ActorContextDecorator
{
    public AutoDownDecorator(IContext context) : base(context)
    {
        Context = context;
    }

    public IContext Context { get; }

    public override async Task Receive(MessageEnvelope envelope)
    {
        switch (envelope.Message)
        {
            case Started:
                Context.SetReceiveTimeout(TimeSpan.FromSeconds(1));
                break;
            case ReceiveTimeoutMessage:
                Context.Poison(Context.Self);
                break;
            default:
                break;
        };

        await base.Receive(envelope);
    }
}
