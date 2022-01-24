var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenTelemetryTracing(builder =>
{
    builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                              .AddService(AppDomain.CurrentDomain.FriendlyName))
           .SetSampler(new AlwaysOnSampler())
           .AddSource(ProtoTags.ActivitySourceName)
           .AddJaegerExporter()
           .AddAspNetCoreInstrumentation();
});

builder.Host.UseProtoActorCluster((sp, option) =>
{
    option.ClusterName = "Sample";
});
builder.Host.UseProtoActorLogging();
builder.Host.UseProtoActorOpenTelemetry();

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

await app.RunAsync();
