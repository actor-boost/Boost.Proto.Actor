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

        public override async Task Receive(MessageEnvelope envelope)
        {
            var pid = Context.Self;
            var message = envelope.Message;
            var header = envelope.Header;

            Logger.LogInformation("{actor}@{message}{header}", pid, message.GetType().Name, header);

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

        public override PID SpawnNamed(Props props, string name)
        {
            var pid = base.SpawnNamed(props, name);

            Logger.LogInformation("{actor}@{message}", pid, "SpawnActor");

            return pid;
        }
    }
}
