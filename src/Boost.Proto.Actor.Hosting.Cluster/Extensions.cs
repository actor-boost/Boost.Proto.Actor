using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.MessagePackSerializer;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Consul;
using Proto.Cluster.Kubernetes;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.Remote;
using Proto.Remote.GrpcNet;

namespace Boost.Proto.Actor.Hosting.Cluster;


public static partial class Extensions
{
    public static IHostBuilder UseProtoActorCluster(this IHostBuilder host,
                                                    Action<Options, IServiceProvider>? option = null,
                                                    string optionPath = "Boost:Actor:Cluster")
    {
        option ??= (o, sp) => { };

        Action<Options, IServiceProvider> optionPost = (o, sp) =>
        {
            option(o, sp);

            o.AdvertisedHost = Environment.GetEnvironmentVariable("PROTO_ADVERTISED_HOST") switch
            {
                null => "127.0.0.1",
                var m => m,
            };

            o.Provider ??= o.AdvertisedHost switch
            {
                "127.0.0.1" => ClusterProviderType.Local,
                _ => ClusterProviderType.Kubernetes,
            };
        };

        host.ConfigureServices((context, services) =>
        {
            services.AddMessagePack();
            services.AddProtoActor();
            services.AddHostedService<HostedService>();

            services.AddOptions<Options>()
                    .BindConfiguration(optionPath)
                    .PostConfigure(optionPost);

            services.AddSingleton(sp => new FuncActorSystem(sp.GetRequiredService<IOptions<Options>>().Value.FuncActorSystem));
            services.AddSingleton(sp => new FuncActorSystemConfig(sp.GetRequiredService<IOptions<Options>>().Value.FuncActorSystemConfig));
            services.AddSingleton(sp => new FuncRootContext(sp.GetRequiredService<IOptions<Options>>().Value.FuncRootContext));
            services.AddSingleton(sp => new FuncIRootContext(sp.GetRequiredService<IOptions<Options>>().Value.FuncIRootContext));
            services.AddSingleton(sp => new FuncActorSystemStart(sp.GetRequiredService<IOptions<Options>>().Value.FuncActorSystemStart));

            services.AddSingleton(sp => KubernetesClientConfiguration.InClusterConfig());
            services.AddSingleton<IKubernetes>(sp => new Kubernetes(sp.GetRequiredService<KubernetesClientConfiguration>()));
            services.AddSingleton(sp => sp.GetRequiredService<IRootContext>().System.Cluster());

            services.AddSingleton<IClusterProvider>(sp =>
            {
                return sp.GetRequiredService<IOptions<Options>>().Value.Provider switch
                {
                    ClusterProviderType.Kubernetes => ActivatorUtilities.CreateInstance<KubernetesProvider>(sp),
                    ClusterProviderType.Consul => ActivatorUtilities.CreateInstance<ConsulProvider>(sp, new ConsulProviderConfig()),
                    _ => new TestProvider(new TestProviderOptions(), new InMemAgent())
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetRequiredService<IOptions<Options>>().Value;

                return option.Provider switch
                {
                    ClusterProviderType.Local => GrpcNetRemoteConfig.BindToLocalhost(),
                    _ => GrpcNetRemoteConfig.BindToAllInterfaces(option.AdvertisedHost)
                                            .WithProtoMessages(option.ProtoMessages.ToArray())
                                            .WithSerializer(11, -50, sp.GetRequiredService<MessagePackSerializer.MessagePackSerializer>())
                                            .WithLogLevelForDeserializationErrors(LogLevel.Critical)
                                            .WithRemoteDiagnostics(true)
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetRequiredService<IOptions<Options>>().Value;
                var clusterKinds = sp.GetRequiredService<IEnumerable<ClusterKind>>();
                return option.FuncClusterConfig.Invoke(
                    ClusterConfig.Setup(option.Name,
                                        sp.GetRequiredService<IClusterProvider>(),
                                        new PartitionIdentityLookup())
                                 .WithClusterKinds(option.ClusterKinds.ToArray())
                                 .WithClusterKinds(clusterKinds?.ToArray() ?? Array.Empty<ClusterKind>()));
            });

            services.AddSingleton<FuncActorSystem>(sp =>
            {
                var option = sp.GetRequiredService<IOptions<Options>>().Value;
                var clusterConfig = sp.GetRequiredService<ClusterConfig>();

                return x =>
                {
                    var y = option.RemoteProvider switch
                    {
                        _ => x.WithRemote(sp.GetRequiredService<GrpcNetRemoteConfig>())
                    };

                    return y.WithCluster(clusterConfig);
                };
            });

            services.AddSingleton<FuncActorSystemConfig>(sp =>
            {
                var option = sp.GetRequiredService<IOptions<Options>>().Value;

                return x => x.WithDeveloperSupervisionLogging(true)
                             .WithDeadLetterRequestLogging(true)
                             .WithDeveloperThreadPoolStatsLogging(true)
                             .WithDeveloperReceiveLogging(TimeSpan.FromSeconds(5));
            });
        });

        return host;
    }
}
