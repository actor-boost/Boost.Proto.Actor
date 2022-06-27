using Boost.Proto.Actor.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;

namespace Boost.Proto.Actor.Decorators
{
    public class LoggerActorContextDecorator : ActorContextDecorator
    {
        public IContext Context { get; }
        public ILogger Logger { get; }

        public LoggerActorContextDecorator(IServiceProvider serviceProvider, IContext context) : base(context)
        {
            Context = context;
            Logger = serviceProvider.GetRequiredService<ILogger<LoggerActorContextDecorator>>();
        }

        public override void Respond(object message)
        {
            if (message is not IIgnoredLogMessage)
            {
                Logger.LogInformation("Respond {@Message}", message);
            }

            base.Respond(message);
        }

        public override async Task Receive(MessageEnvelope envelope)
        {
            var message = envelope.Message;

            if (message is not IIgnoredLogMessage)
            {
                Logger.LogInformation("Receive {@Message}", message);
            }

            try
            {
                await base.Receive(envelope);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "");
                throw;
            }
        }

        public override PID SpawnNamed(Props props, string name, Action<IContext>? callback = null) 
        {
            var pid = base.SpawnNamed(props, name, callback);

            Logger.LogInformation("Spawn {pid}", pid);

            return pid;
        }
    }
}
