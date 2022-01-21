using Google.Protobuf.Reflection;
using Microsoft.Extensions.Configuration;
using Proto;
using Proto.Cluster;
using ProtoClutser = Proto.Cluster.Cluster;

namespace Boost.Proto.Actor.Hosting.Cluster;

public class HostOption
{
    public HostOption(IConfiguration configuration)
    {
        Configuration = configuration;
        AdvertisedHost = Environment.GetEnvironmentVariable("PROTO_ADVERTISED_HOST") switch
        {
            null => "127.0.0.1",
            var m => m,
        };

        ClusterProvider = AdvertisedHost switch
        {
            "127.0.0.1" => ClusterProviderType.Local,
            _ => ClusterProviderType.Kubernetes,
        };

        ClusterProvider = configuration["ProtoActor:Cluster"] switch
        {
            string x when x.ToUpper() is "CONSUL" => ClusterProviderType.Consul,
            _ => ClusterProvider
        };
    }

    public string ClusterName { get; set; }
    public RemoteProviderType RemoteProvider { get; set; } = RemoteProviderType.GrpcNet;
    public ClusterProviderType ClusterProvider { get; set; }
    public IList<ClusterKind> ClusterKinds { get; } = new List<ClusterKind>();
    public IList<FileDescriptor> ProtoMessages { get; } = new List<FileDescriptor>();
    public IConfiguration Configuration { get; }
    public string AdvertisedHost { get; set; }
    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; } = _ => _;
    public Func<ActorSystem, ActorSystem> FuncActorSystem { get; set; } = _ => _;
    public Func<RootContext, RootContext> FuncRootContext { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncIRootContext { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncActorSystemStart { get; set; } = _ => _;
    public Func<ProtoClutser, string, IMemberStrategy> MemberStrategyBuilder { get; set; } = (a, b) => null;
}
