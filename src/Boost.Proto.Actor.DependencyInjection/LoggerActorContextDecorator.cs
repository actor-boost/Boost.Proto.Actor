using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;

namespace Boost.Proto.Actor.DependencyInjection
{
    public class LoggerActorContextDecorator : ActorContextDecorator
    {
        public IContext Context { get; }
        public ILogger Logger { get; }

        public LoggerActorContextDecorator(IContext context, IServiceProvider serviceProvider) : base(context)
        {
            Context = context;
            Logger = serviceProvider.GetRequiredService<ILogger<LoggerActorContextDecorator>>();
        }

        public override async Task Receive(MessageEnvelope envelope)
        {
            var pid = Context.Self;
            var message = envelope.Message;

            Logger.LogInformation($"{pid}@{message.GetType().Name}");

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

            Logger.LogInformation($"{pid}@SpawnActor");

            return pid;
        }
    }
}
