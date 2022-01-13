using Boost.Proto.Actor.Hosting.Cluster.SampleApp.Actors;
using Proto.Cluster;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Host.UseProtoActorCluster((sp, option) =>
{
    option.ClusterName = "Sample";
    option.ClusterKinds.Add(("HelloActor", sp.GetService<IPropsFactory<HelloActor>>().Create()));
});

var app = builder.Build();

app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();
app.Redirect("/", "/swagger");
app.MapGet("/hello", async (Cluster cluster) =>
 {
     var cts = new CancellationTokenSource();
     return await cluster.RequestAsync<int>("1", "HelloActor", 1, cts.Token);
 });

await app.RunAsync();
