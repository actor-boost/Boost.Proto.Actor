using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Cluster;
using Boost.Proto.Actor.Hosting.Logging;
using Boost.Proto.Actor.Hosting.OpenTelemetry;
using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Messages;
using Boost.Proto.Actor.Opentelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Proto;
using Proto.Cluster;

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
});
builder.Host.UseProtoActorLogging((sp, option) => { });

builder.Host.UseProtoActorOpenTelemetry((sp, option) => { });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/Hello", async (Cluster cluster, IRootContext root) =>
{
    var cts = new CancellationTokenSource();
    return await cluster.RequestAsync<HelloResponse>("C7282A09-792A-40F4-9263-3438546D67B9", "HelloActor", new HelloRequest(), root, cts.Token);
});

app.Run();
