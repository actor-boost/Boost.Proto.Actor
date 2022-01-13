using Boost.Proto.Actor.DependencyInjection;
using Google.Protobuf.Reflection;
using Microsoft.Extensions.Configuration;
using Proto;

namespace Boost.Proto.Actor.Hosting.Cluster;

public class ProtoActorClusterOption : IFuncActorSystemConfig, IFuncActorSystem, IActorSystemStart
{
    public ProtoActorClusterOption(IConfiguration configuration)
    {
        Configuration = configuration;
        AdvertisedHost = Environment.GetEnvironmentVariable("PROTO_ADVERTISED_HOST") switch
        {
            null => "127.0.0.1",
            var m => m,
        };
        ClusterProvider = AdvertisedHost switch
        {
            "127.0.0.1" => "local",
            _ => "k8s",
        };
    }

    public string ClusterName { get; set; }

    public string ClusterProvider { get; set; }

    public IEnumerable<(string, Props)> ClusterKinds { get; set; }
        = Array.Empty<(string, Props)>();

    public IEnumerable<FileDescriptor> ProtoMessages { get; set; }
        = Array.Empty<FileDescriptor>();

    public IConfiguration Configuration { get; }

    public string AdvertisedHost { get; set; }

    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; }
           = _ => _;

    public Func<ActorSystem, ActorSystem> FuncSystem { get; set; }
        = _ => _;

    public Action<IServiceProvider, IRootContext> ActorSystemStart { get; set; }
        = (sp, ctx) => { };
}
