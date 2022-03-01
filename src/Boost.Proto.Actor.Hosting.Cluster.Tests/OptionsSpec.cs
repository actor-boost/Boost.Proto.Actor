using System;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.Toolkit.HighPerformance;
using Xunit;
using ClusterOptions = Boost.Proto.Actor.Hosting.Cluster.Options;

namespace Boost.Proto.Actor.Hosting.Cluster.Tests;

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
                ""Name"" : ""UnitTest"",
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
                       .UseProtoActorCluster()
                       .Build();

        await host.StartAsync();

        var option = host.Services.GetRequiredService<IOptions<ClusterOptions>>().Value;

        option.Name.Should().Be("UnitTest");
        option.Provider.Should().Be(ClusterProviderType.Local);

        await host.StopAsync();
    }
}
