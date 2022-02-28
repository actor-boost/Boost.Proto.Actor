using System;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Boost.Proto.Actor.MessagePackSerializer.Tests;


public record Hello(Guid World, string Message);


public class MessagePackSerializerSpec
{
    [Theory, AutoData]
    public async Task SerializeAndDeserializeAsync(Hello msg)
    {
        // Arrange
        var host = Host.CreateDefaultBuilder()
                       .ConfigureServices(services => services.AddMessagePack())
                       .Build();

        await host.StartAsync();

        var serializer = host.Services.GetRequiredService<MessagePackSerializer>();

        // Act

        var dto = serializer.Serialize(msg);

        var ret = serializer.Deserialize(dto, string.Empty);

        ret.Should().Be(msg);


    }
}
