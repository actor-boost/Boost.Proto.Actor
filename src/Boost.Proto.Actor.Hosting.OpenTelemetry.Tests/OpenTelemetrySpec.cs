using System;
using System.Threading;
using System.Threading.Tasks;
using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Cluster;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using Proto;
using Proto.Cluster;
using Xunit;
using static LanguageExt.Prelude;
using Cluster1 = Proto.Cluster.Cluster;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.Tests;

public record OpenTelemetrySpec
{
    [Fact]
    public async Task SimpleCheck()
    {
        var host = Host.CreateDefaultBuilder()
                       .UseProtoActorCluster((o, sp) =>
                       {
                           o.Name = "test";
                           o.Provider = ClusterProviderType.Local;
                           o.ClusterKinds.Add(new("HelloActors",
                               sp.GetRequiredService<IPropsFactory<HelloActor>>()
                                 .Create(new PID("nonhost", "WorldActor"))));

                           o.FuncRootContext = root =>
                           {
                               root.SpawnNamed(sp.GetRequiredService<IPropsFactory<WorldActor>>()
                                                 .Create(), "WorldActor");
                               return root;
                           };

                       })
                       .UseProtoActorOpenTelemetry()
                       .ConfigureServices(services =>
                       {
                           services.AddOpenTelemetryTracing(builder =>
                           {
                               builder.AddJaegerExporter()
                                      .AddSource("Proto.Actor");
                           });
                       })

                       .Build();

        await host.StartAsync();

        var cluster = host.Services.GetRequiredService<Cluster1>();
        var root = host.Services.GetRequiredService<IRootContext>();
        using var cts = new CancellationTokenSource(10 * sec);

        var ret = await cluster.RequestAsync<Unit>("1", "HelloActors", new Hi(), root, cts.Token);
        //if (ret is null) throw new TimeoutException();

        await Task.Delay(TimeSpan.FromSeconds(5));
        await host.StopAsync();
    }
}
