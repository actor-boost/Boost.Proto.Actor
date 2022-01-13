using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Cluster;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Testing;
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
        });

        return host;
    }
}
