using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Boost.Proto.Actor.DependencyInjection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.HighPerformance;
using Proto;
using Proto.Cluster;
using Xunit;
using ClusterOptions = Boost.Proto.Actor.Hosting.Cluster.Options;
using ProtoCluster = global::Proto.Cluster.Cluster;
using static LanguageExt.Prelude;

namespace Boost.Proto.Actor.Hosting.Cluster.Tests;

public class TestActor : IActor
{
    public Task ReceiveAsync(IContext context) => context.Message switch
    {
        "Hello" => Task.Run(() => context.Respond("World")),
        _ => Task.CompletedTask
    };
}

public class OptionsSpec
{
    [Fact]
    public async Task Test1Async()
    {
        using var appsettingsStream = Encoding.Default.GetBytes(@"
{
    ""Boost"" : {
        ""Actor"" : {
            ""Cluster"" : {
                ""Name"" : ""UnitTest1"",
                ""Provider"" : ""Local""
            }
        }
    }

}").AsMemory().AsStream();

        var host = Host.CreateDefaultBuilder()
                       .ConfigureAppConfiguration(config =>
                       {
                           config.AddJsonStream(appsettingsStream);
                       })
                       .UseProtoActorCluster((option, sp) =>
                       {
                           option.Name = "OverrideClusterName";
                           option.ClusterKinds.Add(new
                           (
                               "TestActor",
                               sp.GetRequiredService<IPropsFactory<TestActor>>().Create()
                           ));
                       })
                       .Build();

        await host.StartAsync();

        var option = host.Services.GetRequiredService<IOptions<ClusterOptions>>().Value;

        option.Name.Should().Be("OverrideClusterName");
        option.Provider.Should().Be(ClusterProviderType.Local);
        option.ClusterKinds.Should().HaveCount(1);
        option.ProtoMessages.Should().HaveCount(0);
        option.AdvertisedHost.Should().Be("127.0.0.1");
        option.RemoteProvider.Should().Be(RemoteProviderType.GrpcNet);

        var cluster = host.Services.GetRequiredService<ProtoCluster>();

        var cts = new CancellationTokenSource(3 * sec);

        var ret = await cluster.RequestAsync<string>("1",
                                                     "TestActor",
                                                     "Hello",
                                                     cts.Token);

        ret.Should().Be("World");

        await host.StopAsync();
    }
}
