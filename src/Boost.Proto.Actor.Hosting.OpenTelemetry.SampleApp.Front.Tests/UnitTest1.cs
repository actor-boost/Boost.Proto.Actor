using System.Threading.Tasks;
using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Messages;
using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Models;
using FluentAssertions;
using Flurl.Http;
using Microsoft.Extensions.DependencyInjection;
using Proto;
using Proto.Cluster;
using Xunit;
using static LanguageExt.Prelude;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Front.Tests
{
    public class UnitTest1
    {
        [Fact]
        public async Task Test1()
        {
            var sut = new App().WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    services.AddSingleton(sp => new ClusterKind("HelloActor",
                        Props.FromFunc(ctx => ctx.Message switch
                        {
                            HelloRequest msg => fun(() =>
                            {
                                ctx.Respond(new HelloResponse("1", new Name("Tester")));
                                return Task.CompletedTask;
                            })(),
                            _ => Task.CompletedTask
                        })));
                });
            });

            var client = new FlurlClient(sut.CreateClient());

            var ret = await client.Request("/Hello")
                                  .GetJsonAsync<HelloResponse>();

            ret.Should().BeEquivalentTo(new
            {
                Count = "1",
                Name = new
                {
                    Value = "Tester"
                }
            });
        }
    }
}
