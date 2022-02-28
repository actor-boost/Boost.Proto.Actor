using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.MessagePackSerializer;
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
            services.AddMessagePack();
            services.AddProtoActor();
            services.AddHostedService<HostedService>();

            services.AddSingleton(sp =>
            {
                var ret = ActivatorUtilities.CreateInstance<HostOption>(sp);
                option?.Invoke(sp, ret);
                return ret;
            });

            services.AddSingleton(sp => new FuncActorSystem(sp.GetRequiredService<HostOption>().FuncActorSystem));
            services.AddSingleton(sp => new FuncActorSystemConfig(sp.GetRequiredService<HostOption>().FuncActorSystemConfig));
            services.AddSingleton(sp => new FuncRootContext(sp.GetRequiredService<HostOption>().FuncRootContext));
            services.AddSingleton(sp => new FuncIRootContext(sp.GetRequiredService<HostOption>().FuncIRootContext));
            services.AddSingleton(sp => new FuncActorSystemStart(sp.GetRequiredService<HostOption>().FuncActorSystemStart));

            services.AddSingleton(sp => KubernetesClientConfiguration.InClusterConfig());
            services.AddSingleton<IKubernetes>(sp => new Kubernetes(sp.GetRequiredService<KubernetesClientConfiguration>()));
            services.AddSingleton(sp => sp.GetRequiredService<IRootContext>().System.Cluster());

            services.AddSingleton<IClusterProvider>(sp =>
            {
                return sp.GetRequiredService<HostOption>().ClusterProvider switch
                {
                    ClusterProviderType.Kubernetes => ActivatorUtilities.CreateInstance<KubernetesProvider>(sp),
                    ClusterProviderType.Consul => ActivatorUtilities.CreateInstance<ConsulProvider>(sp, new ConsulProviderConfig()),
                    _ => new TestProvider(new TestProviderOptions(), new InMemAgent())
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetRequiredService<HostOption>();

                return option.ClusterProvider switch
                {
                    ClusterProviderType.Local => GrpcNetRemoteConfig.BindToLocalhost(),
                    _ => GrpcNetRemoteConfig.BindToAllInterfaces(option.AdvertisedHost)
                                            .WithProtoMessages(option.ProtoMessages.ToArray())
                                            .WithSerializer(11, -50, sp.GetRequiredService<MessagePackSerializer.MessagePackSerializer>())
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetRequiredService<HostOption>();

                return option.ClusterProvider switch
                {
                    ClusterProviderType.Local => GrpcCoreRemoteConfig.BindToLocalhost(),
                    _ => GrpcCoreRemoteConfig.BindToAllInterfaces(option.AdvertisedHost)
                                             .WithProtoMessages(option.ProtoMessages.ToArray())
                                             .WithSerializer(11, -50, sp.GetRequiredService<MessagePackSerializer.MessagePackSerializer>())
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetRequiredService<HostOption>();
                var clusterKinds = sp.GetRequiredService<IEnumerable<ClusterKind>>();
                return ClusterConfig.Setup(option.ClusterName,
                                           sp.GetRequiredService<IClusterProvider>(),
                                           new PartitionIdentityLookup())
                                    .WithClusterKinds(option.ClusterKinds.ToArray())
                                    .WithClusterKinds(clusterKinds?.ToArray() ?? Array.Empty<ClusterKind>());
            });

            services.AddSingleton<FuncActorSystem>(sp =>
            {
                var option = sp.GetRequiredService<HostOption>();
                var clusterConfig = sp.GetRequiredService<ClusterConfig>();

                return x =>
                {
                    var y = option.RemoteProvider switch
                    {
                        RemoteProviderType.GrpcCore => x.WithRemote(sp.GetRequiredService<GrpcCoreRemoteConfig>()),
                        _ => x.WithRemote(sp.GetRequiredService<GrpcNetRemoteConfig>())
                    };

                    return y.WithCluster(clusterConfig);
                };
            });

            services.AddSingleton<FuncActorSystemConfig>(sp =>
            {
                var option = sp.GetRequiredService<HostOption>();

                return x => x.WithDeveloperSupervisionLogging(true)
                             .WithDeadLetterRequestLogging(true)
                             .WithDeveloperThreadPoolStatsLogging(true)
                             .WithDeveloperReceiveLogging(TimeSpan.FromSeconds(5));
            });
        });

        return host;
    }
}
