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
            services.AddSingleton(sp => new FuncActorSystemStart(sp.GetService<HostOption>().FuncActorSystemStart));

            services.AddSingleton(sp => KubernetesClientConfiguration.InClusterConfig());
            services.AddSingleton<IKubernetes>(sp => new Kubernetes(sp.GetService<KubernetesClientConfiguration>()));
            services.AddSingleton(sp => sp.GetService<IRootContext>().System.Cluster());

            services.AddSingleton<IClusterProvider>(sp =>
            {
                return sp.GetService<HostOption>().ClusterProvider switch
                {
                    ClusterProviderType.Kubernetes => ActivatorUtilities.CreateInstance<KubernetesProvider>(sp),
                    _ => new TestProvider(new TestProviderOptions(), new InMemAgent())
                };
            });

            services.AddSingleton<GrpcNetRemoteConfig>(sp =>
            {
                var option = sp.GetService<HostOption>();

                return option.ClusterProvider switch
                {
                    ClusterProviderType.Kubernetes => GrpcNetRemoteConfig.BindToAllInterfaces(option.AdvertisedHost)
                                                                         .WithProtoMessages(option.ProtoMessages.ToArray()),
                    _ => GrpcNetRemoteConfig.BindToLocalhost()
                };
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetService<HostOption>();
                return ClusterConfig.Setup(option.ClusterName,
                                           sp.GetService<IClusterProvider>(),
                                           new PartitionIdentityLookup())
                                    .WithClusterKinds(option.ClusterKinds.ToArray());
            });

            services.AddSingleton(sp =>
            {
                var clusterConfig = sp.GetService<ClusterConfig>();
                var remoteConfig = sp.GetService<GrpcNetRemoteConfig>();

                return new FuncActorSystem(x => x.WithRemote(remoteConfig)
                                                 .WithCluster(clusterConfig));
            });

            services.AddSingleton(sp =>
            {
                var option = sp.GetService<HostOption>();

                return new FuncActorSystemConfig(
                   x => x.WithDeveloperSupervisionLogging(true)
                         .WithDeadLetterRequestLogging(true)
                         .WithDeveloperThreadPoolStatsLogging(true)
                         .WithDeveloperReceiveLogging(TimeSpan.FromSeconds(5)));
            });
        });

        return host;
    }
}
