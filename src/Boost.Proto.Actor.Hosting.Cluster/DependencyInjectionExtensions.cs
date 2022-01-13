using Boost.Proto.Actor.DependencyInjection;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Kubernetes;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.Remote.GrpcNet;
using Proto.Remote;

namespace Boost.Proto.Actor.Hosting.Cluster;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddProtoActor(this IServiceCollection services)
    {
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
        
        services.AddSingleton(sp =>
        {
            var funcSystem = (ActorSystem x) =>
            {
                var option = sp.GetService<ProtoActorClusterOption>();
                var clusterConfig = sp.GetService<ClusterConfig>();
                var remoteConfig = sp.GetService<GrpcNetRemoteConfig>();

                var ret = option.FuncSystem(x);

                return ret.WithRemote(remoteConfig)
                          .WithCluster(clusterConfig);
            };

            var funcConfig = (ActorSystemConfig x) =>
            {
                Log.SetLoggerFactory(sp.GetRequiredService<ILoggerFactory>());
                var option = sp.GetService<ProtoActorClusterOption>();

                var ret = option.FuncActorSystemConfig(x)
                                .WithDeveloperSupervisionLogging(true)
                                .WithDeadLetterRequestLogging(true)
                                .WithDeveloperThreadPoolStatsLogging(true)
                                .WithDeveloperReceiveLogging(TimeSpan.FromSeconds(5));

                return ret;
            };
            return funcSystem(new ActorSystem(funcConfig?.Invoke(ActorSystemConfig.Setup())));
        });

        services.AddSingleton(typeof(IPropsFactory<>), typeof(PropsFactory<>));
        services.AddSingleton<IRootContext>(sp => sp.GetService<ActorSystem>().Root);
        return services;
    }

    
}
