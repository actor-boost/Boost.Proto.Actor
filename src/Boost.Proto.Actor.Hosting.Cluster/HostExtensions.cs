using Boost.Proto.Actor.DependencyInjection;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Kubernetes;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.Remote;
using Proto.Remote.GrpcNet;

namespace Boost.Proto.Actor.Hosting.Cluster;

public static class HostExtensions
{
    public static IHostBuilder UseProtoActorCluster(this IHostBuilder host,
                                                    Action<IServiceProvider, ProtoActorClusterOption> option)
    {
        host.ConfigureServices((context, services) =>
        {
            services.AddSingleton(sp =>
            {
                var ret = sp.CreateInstance<ProtoActorClusterOption>();
                option?.Invoke(sp, ret);
                return ret;
            });

            services.AddSingleton<IFuncActorSystem>(sp => sp.GetService<ProtoActorClusterOption>());
            services.AddSingleton<IFuncActorSystemConfig>(sp => sp.GetService<ProtoActorClusterOption>());
            services.AddSingleton<IActorSystemStart>(sp => sp.GetService<ProtoActorClusterOption>());

            services.AddProtoActor();
            services.AddHostedService<ProtoActorHostedService>();

            services.AddSingleton(sp => KubernetesClientConfiguration.InClusterConfig());
            services.AddSingleton<IKubernetes>(sp => new Kubernetes(sp.GetService<KubernetesClientConfiguration>()));
            services.AddSingleton(sp => sp.GetService<ActorSystem>().Cluster());

            services.AddSingleton<IClusterProvider>(sp =>
            {
                return sp.GetService<ProtoActorClusterOption>().ClusterProvider switch
                {
                    ClusterProviderType.Kubernetes => sp.CreateInstance<KubernetesProvider>(),
                    _ => new TestProvider(new TestProviderOptions(), new InMemAgent())
                };
            });

            services.AddSingleton<GrpcNetRemoteConfig>(sp =>
            {
                var option = sp.GetService<ProtoActorClusterOption>();

                return option.ClusterProvider switch
                {
                    ClusterProviderType.Kubernetes => GrpcNetRemoteConfig.BindToAllInterfaces(option.AdvertisedHost)
                                                                         .WithProtoMessages(option.ProtoMessages.ToArray()),
                    _ => GrpcNetRemoteConfig.BindToLocalhost()
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetService<ProtoActorClusterOption>();
                return ClusterConfig.Setup(option.ClusterName,
                                           sp.GetService<IClusterProvider>(),
                                           new PartitionIdentityLookup())
                                    .WithClusterKinds(option.ClusterKinds.ToArray());
            });

            services.AddSingleton<IFuncActorSystem>(sp =>
            {
                var clusterConfig = sp.GetService<ClusterConfig>();
                var remoteConfig = sp.GetService<GrpcNetRemoteConfig>();

                return new FuncClusterActorSystem
                {
                    FuncSystem = x => x.WithRemote(remoteConfig)
                                       .WithCluster(clusterConfig)
                };
            });

            services.AddSingleton<IFuncActorSystemConfig>(sp =>
            {
                var option = sp.GetService<ProtoActorClusterOption>();

                return new FuncClusterActorSystemConfig
                {
                    FuncActorSystemConfig = x => x.WithDeveloperSupervisionLogging(true)
                                                  .WithDeadLetterRequestLogging(true)
                                                  .WithDeveloperThreadPoolStatsLogging(true)
                                                  .WithDeveloperReceiveLogging(TimeSpan.FromSeconds(5))
                };
            });
        });

        return host;
    }
}
