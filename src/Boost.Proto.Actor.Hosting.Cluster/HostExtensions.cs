using Boost.Proto.Actor.DependencyInjection;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Consul;
using Proto.Cluster.Kubernetes;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.Remote;
using Proto.Remote.GrpcCore;
using Proto.Remote.GrpcNet;

namespace Boost.Proto.Actor.Hosting.Cluster;


public static class HostExtensions
{
    public static IHostBuilder UseProtoActorCluster(this IHostBuilder host,
                                                    Action<IServiceProvider, HostOption> option)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddProtoActor();
            services.AddHostedService<HostedService>();

            services.AddSingleton(sp =>
            {
                var ret = ActivatorUtilities.CreateInstance<HostOption>(sp);
                option?.Invoke(sp, ret);
                return ret;
            });

            services.AddSingleton(sp => new FuncActorSystem(sp.GetService<HostOption>().FuncActorSystem));
            services.AddSingleton(sp => new FuncActorSystemConfig(sp.GetService<HostOption>().FuncActorSystemConfig));
            services.AddSingleton(sp => new FuncRootContext(sp.GetService<HostOption>().FuncRootContext));
            services.AddSingleton(sp => new FuncIRootContext(sp.GetService<HostOption>().FuncIRootContext));
            services.AddSingleton(sp => new FuncActorSystemStart(sp.GetService<HostOption>().FuncActorSystemStart));

            services.AddSingleton(sp => KubernetesClientConfiguration.InClusterConfig());
            services.AddSingleton<IKubernetes>(sp => new Kubernetes(sp.GetService<KubernetesClientConfiguration>()));
            services.AddSingleton(sp => sp.GetService<IRootContext>().System.Cluster());

            services.AddSingleton<IClusterProvider>(sp =>
            {
                return sp.GetService<HostOption>().ClusterProvider switch
                {
                    ClusterProviderType.Kubernetes => ActivatorUtilities.CreateInstance<KubernetesProvider>(sp),
                    ClusterProviderType.Consul => ActivatorUtilities.CreateInstance<ConsulProvider>(sp, new ConsulProviderConfig()),
                    _ => new TestProvider(new TestProviderOptions(), new InMemAgent())
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetService<HostOption>();

                return option.ClusterProvider switch
                {
                    ClusterProviderType.Local => GrpcNetRemoteConfig.BindToLocalhost(),
                    _ => GrpcNetRemoteConfig.BindToAllInterfaces(option.AdvertisedHost)
                                            .WithProtoMessages(option.ProtoMessages.ToArray())
                                            .WithSerializer(10, -50, new ProtobufNetSerializer.ProtobufNetSerializer())
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetService<HostOption>();

                return option.ClusterProvider switch
                {
                    ClusterProviderType.Local => GrpcCoreRemoteConfig.BindToLocalhost(),
                    _ => GrpcCoreRemoteConfig.BindToAllInterfaces(option.AdvertisedHost)
                                             .WithProtoMessages(option.ProtoMessages.ToArray())
                                             .WithSerializer(10, -50, new ProtobufNetSerializer.ProtobufNetSerializer())
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetService<HostOption>();
                var clusterKinds = sp.GetService<IEnumerable<ClusterKind>>();
                return ClusterConfig.Setup(option.ClusterName,
                                           sp.GetService<IClusterProvider>(),
                                           new PartitionIdentityLookup())
                                    .WithClusterKinds(option.ClusterKinds.ToArray())
                                    .WithClusterKinds(clusterKinds?.ToArray() ?? Array.Empty<ClusterKind>());
            });

            services.AddSingleton<FuncActorSystem>(sp =>
            {
                var option = sp.GetService<HostOption>();
                var clusterConfig = sp.GetService<ClusterConfig>();

                return x =>
                {
                    var y = option.RemoteProvider switch
                    {
                        RemoteProviderType.GrpcCore => x.WithRemote(sp.GetService<GrpcCoreRemoteConfig>()),
                        _ => x.WithRemote(sp.GetService<GrpcNetRemoteConfig>())
                    };

                    return y.WithCluster(clusterConfig);
                };
            });

            services.AddSingleton<FuncActorSystemConfig>(sp =>
            {
                var option = sp.GetService<HostOption>();

                return x => x.WithDeveloperSupervisionLogging(true)
                             .WithDeadLetterRequestLogging(true)
                             .WithDeveloperThreadPoolStatsLogging(true)
                             .WithDeveloperReceiveLogging(TimeSpan.FromSeconds(5));
            });
        });

        return host;
    }
}
