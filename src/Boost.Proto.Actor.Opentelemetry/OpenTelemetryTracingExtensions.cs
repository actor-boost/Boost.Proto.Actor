using System.Diagnostics;
using OpenTelemetry.Trace;

namespace Boost.Proto.Actor.Opentelemetry
{
    public delegate void ActivitySetup(Activity? activity, object message);

    public static class OpenTelemetryTracingExtensions
    {
        public static TracerProviderBuilder AddProtoActorInstrumentation(this TracerProviderBuilder builder)
            => builder.AddSource(ProtoTags.ActivitySourceName);

        /// <summary>
        ///     Setup OpenTelemetry send middleware & decorator.
        /// </summary>
        /// <param name="props">props.</param>
        /// <param name="sendActivitySetup">provide a way inject send activity customization according to the message.</param>
        /// <param name="receiveActivitySetup">provide a way inject receive activity customization according to the message.</param>
        /// <returns>props</returns>
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

        /// <summary>
        ///     Adds trace headers to the message envelope, to propagate trace context.
        /// </summary>
        public static Sender OpenTelemetrySenderMiddleware(Sender next)
            => async (context, target, envelope) =>
            {
                var activity = Activity.Current;

                if (activity != null)
                {
                    envelope = envelope.WithHeaders(activity.Context.GetPropagationHeaders());
                }

                await next(context, target, envelope);
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
