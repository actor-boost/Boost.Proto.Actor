using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Cluster;
using Boost.Proto.Actor.Hosting.Logging;
using Boost.Proto.Actor.Hosting.OpenTelemetry;
using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Back.Actors;
using Boost.Proto.Actor.Opentelemetry;
using k8s.KubeConfigModels;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Proto;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                              .AddService(AppDomain.CurrentDomain.FriendlyName))
           .SetSampler(new AlwaysOnSampler())
           .AddSource(ProtoTags.ActivitySourceName)
           .AddConsoleExporter()
           .AddJaegerExporter()
           .AddAspNetCoreInstrumentation();
});

builder.Host.UseProtoActorCluster((sp, option) =>
{
    option.ClusterName = "Sample";
    option.ClusterKinds.Add((nameof(HelloActor), sp.GetService<IPropsFactory<HelloActor>>().Create()));
});
builder.Host.UseProtoActorLogging((sp, option) => { });

builder.Host.UseProtoActorOpenTelemetry((sp, option) => { });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.Run();
