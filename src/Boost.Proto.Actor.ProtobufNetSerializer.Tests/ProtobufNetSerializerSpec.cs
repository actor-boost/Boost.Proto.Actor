using System.Collections.Generic;
using FluentAssertions;
using ProtoBuf;
using Xunit;

namespace Boost.Proto.Actor.ProtobufNetSerializer.Tests;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllPublic)]
public record Hello(string Id, IReadOnlyList<string> Words);

public class ProtobufNetSerializerSpec
{
    [Fact]
    public void Should_Be_Correct()
    {
        var serializer = new ProtobufNetSerializer();

        var hello = new Hello(
            "1",
            new[]
            {
                "World", "Great"
            }
        );

        var packet = serializer.Serialize(hello);

        var ret = serializer.Deserialize(packet, serializer.GetTypeName(hello));

        ret.Should().BeEquivalentTo(hello);
    }
}
