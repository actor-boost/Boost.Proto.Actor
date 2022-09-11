using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using k8s.KubeConfigModels;
using Proto;
using Proto.Cluster;
using Proto.Mailbox;

namespace ClusterSingle.Actor;

public class EchoGrainActor : GrainActor<EchoGrainActor>, IActor
{
    public Task ReceiveAsync(IContext context)
    {
        switch (context.Message)
        {
            case Started:
               // context.SetReceiveTimeout(TimeSpan.FromSeconds(1));
                break;
            case ReceiveTimeout:
                context.Poison(context.Self);
                break;
            default:
                break;
        };

        return context.Message switch
        {
            SystemMessage => Task.CompletedTask,
            ClusterInit => Task.CompletedTask,
            object m => Task.Run(() => context.Respond(m)),
            _ => Task.CompletedTask,
        };
    }
}
