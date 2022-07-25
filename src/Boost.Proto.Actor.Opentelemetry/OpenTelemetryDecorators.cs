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
            => _sendActivitySetup = (activity, message)
                =>
            {
                activity?.SetTag(ProtoTags.ActorType, "<None>");
                sendActivitySetup(activity, message);
            };

        protected override IRootContext WithInnerContext(IRootContext context) => new OpenTelemetryRootContextDecorator(context, _sendActivitySetup);

        //public override void Send(PID target, object message)
        //    => OpenTelemetryMethodsDecorators.Send(target, message, _sendActivitySetup, () => base.Send(target, message));

        //public override void Request(PID target, object message)
        //    => OpenTelemetryMethodsDecorators.Request(target, message, _sendActivitySetup, () => base.Request(target, message));

        //public override void Request(PID target, object message, PID? sender)
        //    => OpenTelemetryMethodsDecorators.Request(target, message, sender, _sendActivitySetup, () => base.Request(target, message, sender));

        //public override Task<T> RequestAsync<T>(PID target, object message, CancellationToken cancellationToken)
        //    => OpenTelemetryMethodsDecorators.RequestAsync(target, message, _sendActivitySetup,
        //        () => base.RequestAsync<T>(target, message, cancellationToken)
        //    );
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
                    var n = names.Length > 2 ? $"{names[0]}/.../{OpenTelemetryMethodsDecorators.Truncate(names[^1], 10)}" : pid.ToString();

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

        //public override void Send(PID target, object message)
        //    => OpenTelemetryMethodsDecorators.Send(target, message, _sendActivitySetup, () => base.Send(target, message));

        //public override Task<T> RequestAsync<T>(PID target, object message, CancellationToken cancellationToken)
        //    => OpenTelemetryMethodsDecorators.RequestAsync(target, message, _sendActivitySetup,
        //        () => base.RequestAsync<T>(target, message, cancellationToken)
        //    );

        //public override void Request(PID target, object message, PID? sender)
        //    => OpenTelemetryMethodsDecorators.Request(target, message, sender, _sendActivitySetup, () => base.Request(target, message, sender));

        //public override void Forward(PID target)
        //    => OpenTelemetryMethodsDecorators.Forward(target, base.Message!, _sendActivitySetup, () => base.Forward(target));

        public override Task Receive(MessageEnvelope envelope)
            => OpenTelemetryMethodsDecorators.Receive(Self, envelope, _receiveActivitySetup, () => base.Receive(envelope));
    }

    internal static class OpenTelemetryMethodsDecorators
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Send(PID target, object message, ActivitySetup sendActivitySetup, Action send)
        {
            using var activity =
                OpenTelemetryHelpers.BuildStartedActivity(Activity.Current?.Context ?? default, "Tell ", message, sendActivitySetup);

            try
            {
                activity?.SetTag(ProtoTags.TargetPID, target.ToString());
                send();
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Request(PID target, object message, ActivitySetup sendActivitySetup, Action request)
        {
            using var activity =
                OpenTelemetryHelpers.BuildStartedActivity(Activity.Current?.Context ?? default, "Ask ", message, sendActivitySetup);

            try
            {
                activity?.SetTag(ProtoTags.TargetPID, target.ToString());
                request();
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Request(PID target, object message, PID? sender, ActivitySetup sendActivitySetup, Action request)
        {
            using var activity =
                OpenTelemetryHelpers.BuildStartedActivity(Activity.Current?.Context ?? default, "Ask ", message, sendActivitySetup);

            try
            {
                activity?.SetTag(ProtoTags.TargetPID, target.ToString());

                if (sender is not null)
                {
                    activity?.SetTag(ProtoTags.SenderPID, sender.ToString());
                }

                request();
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async Task<T> RequestAsync<T>(PID target, object message, ActivitySetup sendActivitySetup, Func<Task<T>> requestAsync)
        {
            using var activity =
                OpenTelemetryHelpers.BuildStartedActivity(Activity.Current?.Context ?? default, "Ask ", message, sendActivitySetup);

            try
            {
                activity?.SetTag(ProtoTags.TargetPID, target.ToString());
                return await requestAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                throw;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void Forward(PID target, object message, ActivitySetup sendActivitySetup, Action forward)
        {
            using var activity =
                OpenTelemetryHelpers.BuildStartedActivity(Activity.Current?.Context ?? default, "Forward ", message, sendActivitySetup);

            try
            {
                activity?.SetTag(ProtoTags.TargetPID, target.ToString());
                forward();
            }
            catch (Exception ex)
            {
                activity?.RecordException(ex);
                activity?.SetStatus(Status.Error);
                throw;
            }
        }

        public static string Truncate(string value, int maxChars)
        {
            const string ellipses = "...";
            return value.Length <= maxChars ? value : value.Substring(0, maxChars - ellipses.Length) + ellipses;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static async Task Receive(string self, MessageEnvelope envelope, ActivitySetup receiveActivitySetup, Func<Task> receive)
        {
            var message = envelope.Message;

            if (message is InfrastructureMessage)
            {
                await receive().ConfigureAwait(false);
                return;
            }

            var propagationContext = envelope.Header.ExtractPropagationContext();

            var names = self.Split('/');
            var name = names.Length > 2 ? $"{names[0]}/.../{Truncate(names[^1], 10)}" : self;

            using var activity =
                OpenTelemetryHelpers.BuildStartedActivity(propagationContext.ActivityContext, $"{name}@", message, receiveActivitySetup);

            try
            {
                if (envelope.Sender is not null)
                {
                    activity?.SetTag(ProtoTags.SenderPID, envelope.Sender.ToString());
                }

                receiveActivitySetup?.Invoke(activity, message);

                await receive().ConfigureAwait(false);
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
