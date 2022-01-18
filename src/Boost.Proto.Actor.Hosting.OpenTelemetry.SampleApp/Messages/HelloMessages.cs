using Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Models;
using ProtoBuf;

namespace Boost.Proto.Actor.Hosting.OpenTelemetry.SampleApp.Messages;

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllPublic)]
public record HelloRequest();

[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllPublic)]
public record HelloResponse(string Count, Name Name);


