using System.Reflection;
using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Cluster;
using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Actors;
using Boost.Proto.Actor.Opentelemetry;
using Boost.Proto.Actor.OpenTelemetry;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
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
           .AddAspNetCoreInstrumentation()
           .AddConsoleExporter();
});

builder.Host.UseProtoActorCluster((sp, option) =>
{
    option.ClusterName = "Sample";
    option.ClusterKinds.Add((nameof(HelloActor), sp.GetService<IPropsFactory<HelloActor>>().Create()));
});

builder.Host.UseProtoActorOpenTelemetry((sp, option) => { });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/Hello", async (Cluster cluster) =>
{
    var cts = new CancellationTokenSource();
    return await cluster.RequestAsync<int>("C7282A09-792A-40F4-9263-3438546D67B9", "HelloActor", 1, cts.Token);
});

app.Run();
