using System;
using Boost.Proto.Actor.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Proto;
using Proto.Cluster;

namespace ClusterSingle.Actor;

public abstract class GrainActor<T> where T : IActor
{
    protected static string GetGetKind() => typeof(T).Name;

    public static ClusterKind GetClusterKind(IServiceProvider sp, Func<Props, Props>? decorator = null, params object[] paramter) =>
        new(GetGetKind(), decorator switch
        {
            null => sp.GetRequiredService<IPropsFactory<T>>().Create(paramter),
            _ => decorator.Invoke(sp.GetRequiredService<IPropsFactory<T>>().Create(paramter))
        });

    public static ClusterIdentity GetClusterIdentity(string identity) =>
        ClusterIdentity.Create(identity, GetGetKind());
}
