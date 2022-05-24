using Google.Protobuf.Reflection;
using Microsoft.Extensions.Configuration;
using Proto;
using Proto.Cluster;
using ProtoClutser = Proto.Cluster.Cluster;

namespace Boost.Proto.Actor.Hosting.Cluster;

public class Options
{
    public string? Name { get; set; }
    public RemoteProviderType RemoteProvider { get; set; } = RemoteProviderType.GrpcNet;
    public ClusterProviderType? Provider { get; set; }
    public IList<ClusterKind> ClusterKinds { get; } = new List<ClusterKind>();
    public IList<FileDescriptor> ProtoMessages { get; } = new List<FileDescriptor>();
    public string? AdvertisedHost { get; set; }
    public TimeSpan GossipRequestTimeout { get; set; } = TimeSpan.FromMilliseconds(500);
    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; } = _ => _;
    public Func<ActorSystem, ActorSystem> FuncActorSystem { get; set; } = _ => _;
    public Func<RootContext, RootContext> FuncRootContext { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncIRootContext { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncActorSystemStart { get; set; } = _ => _;
    public Func<ProtoClutser, string, IMemberStrategy> MemberStrategyBuilder { get; set; } = (a, b) => null;
}
