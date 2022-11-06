using System.Diagnostics;
using OpenTelemetry.Trace;

namespace Boost.Proto.Actor.Opentelemetry
{
    public delegate void ActivitySetup(Activity? activity, object message);

    public static class OpenTelemetryTracingExtensions
    {
        public static TracerProviderBuilder AddProtoActorInstrumentation(this TracerProviderBuilder builder)
            => builder.AddSource(ProtoTags.ActivitySourceName);

        public static Props WithTracing(
            this Props props,
            ActivitySetup? sendActivitySetup = null,
            ActivitySetup? receiveActivitySetup = null
        )
        {
            sendActivitySetup ??= OpenTelemetryHelpers.DefaultSetupActivity!;
            receiveActivitySetup ??= OpenTelemetryHelpers.DefaultSetupActivity!;
            return props
                .WithContextDecorator(ctx => new OpenTelemetryActorContextDecorator(ctx, sendActivitySetup, receiveActivitySetup))
                .WithSenderMiddleware(OpenTelemetrySenderMiddleware);
        }

        public static Sender OpenTelemetrySenderMiddleware(Sender next)
            => (context, target, envelope) =>
            {
                var activity = context.Get<Activity>() ?? Activity.Current;

                if (activity != null)
                {
                    envelope = envelope.WithHeaders(activity.Context.GetPropagationHeaders());
                }

                return next(context, target, envelope);
            };

        public static RootContext WithTracing(this RootContext context) => context;

        public static IRootContext WithTracing(this IRootContext context, ActivitySetup? sendActivitySetup = null)
        {
            sendActivitySetup ??= OpenTelemetryHelpers.DefaultSetupActivity!;

            context.WithSenderMiddleware(OpenTelemetrySenderMiddleware);

            return new OpenTelemetryRootContextDecorator(context, sendActivitySetup);
        }
    }
}
