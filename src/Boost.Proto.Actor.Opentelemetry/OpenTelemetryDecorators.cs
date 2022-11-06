using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OpenTelemetry.Trace;

namespace Boost.Proto.Actor.Opentelemetry
{
    internal class OpenTelemetryRootContextDecorator : RootContextDecorator
    {
        private readonly ActivitySetup _sendActivitySetup;

        public OpenTelemetryRootContextDecorator(IRootContext context, ActivitySetup sendActivitySetup) : base(context)
            => _sendActivitySetup = (activity, message) =>
            {
                activity?.SetTag(ProtoTags.ActorType, "<None>");
                sendActivitySetup(activity, message);
            };
    }

    public class OpenTelemetryActorContextDecorator : ActorContextDecorator
    {
        private static readonly ActivitySource ActivitySource = new(ProtoTags.ActivitySourceName);

        private readonly ActivitySetup _receiveActivitySetup;
        private readonly ActivitySetup _sendActivitySetup;
        private readonly string Self;

        public OpenTelemetryActorContextDecorator(
            IContext context,
            ActivitySetup sendActivitySetup,
            ActivitySetup receiveActivitySetup
        ) : base(context)
        {
            var actorType = context?.Actor?.GetType().Name;
            Self = context?.Self?.ToString();
            _sendActivitySetup = (activity, message) =>
            {
                activity?.SetTag(ProtoTags.ActorType, actorType);
                activity?.SetTag(ProtoTags.ActorPID, Self);
                activity?.SetTag(ProtoTags.SenderPID, Self);
                sendActivitySetup(activity, message);
            };
            _receiveActivitySetup = (activity, message) =>
            {
                activity?.SetTag(ProtoTags.ActorType, actorType);
                activity?.SetTag(ProtoTags.ActorPID, Self);
                activity?.SetTag(ProtoTags.TargetPID, Self);
                receiveActivitySetup(activity, message);
            };
        }

        public override PID SpawnNamed(Props props, string name, Action<IContext>? callback = null)
        {
            if (Activity.Current is not null)
            {
                using var activity = ActivitySource.StartActivity("");
                var pid = base.SpawnNamed(props, name, callback);

                if (activity is not null)
                {
                    var names = pid.ToString().Split('/');
                    var n = names.Length > 2 ? $"{names[0]}/.../" : pid.ToString();

                    activity.DisplayName = $"Spawned {n}";
                }

                return pid;
            }
            else
            {
                return base.SpawnNamed(props, name);
            }
        }

        public override void Respond(object message)
        {
            if (Activity.Current is not null)
            {
                var msg = message.GetType().Name;

                Activity.Current.AddTag(ProtoTags.ResponseType, msg);
                Activity.Current.DisplayName = $"{Activity.Current.DisplayName}:{msg}";
            }

            base.Respond(message);
        }

        public override async Task Receive(MessageEnvelope envelope)
        {
            var message = envelope.Message;

            if (message is InfrastructureMessage)
            {
                await base.Receive(envelope);
                return;
            }

            var propagationContext = envelope.Header.ExtractPropagationContext();
            var name = Self.GetType().Name;

            using var activity =
                OpenTelemetryHelpers.BuildStartedActivity(propagationContext.ActivityContext, $"{name}@", message, _receiveActivitySetup);

            try
            {
                if (envelope.Sender is not null)
                {
                    activity?.SetTag(ProtoTags.SenderPID, envelope.Sender.ToString());
                }

                _receiveActivitySetup?.Invoke(activity, message);

                await base.Receive(envelope);
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                throw;
            }
        }
            
    }

   
}
