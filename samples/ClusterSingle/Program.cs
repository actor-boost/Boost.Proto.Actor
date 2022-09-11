using Boost.Proto.Actor.Decorators;
using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Cluster;
using Boost.Proto.Actor.Hosting.Logging;
using ClusterSingle.Actor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Proto;
using Proto.Cluster;



var host = Host.CreateDefaultBuilder()
               .UseProtoActorCluster((o, sp) =>
               {
                   o.Name = "poc";
                   o.Provider = ClusterProviderType.Local;
                   o.SystemShutdownDelaySec = 0;

                   o.ClusterKinds.Add(EchoGrainActor.GetClusterKind(sp, props => props.WithContextDecorator(c => new AutoDownDecorator(c))));

                   o.FuncActorSystemStartAsync = async (root, next) =>
                   {
                       await next(root);

                       using var cts = new CancellationTokenSource();

                       await root.System.Cluster().RequestAsync<Hello>(
                           clusterIdentity: ClusterIdentity.Create("a", nameof(EchoGrainActor)),
                           message: new Hello(),
                           context: root,
                           ct: cts.Token);

                   };

               })
               .UseProtoActorLogging()
               .Build();

await host.RunAsync();


record Hello();
