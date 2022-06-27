using Microsoft.Extensions.Configuration;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local;

public class HostOption
{
    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; } = _ => _;
    public Func<ActorSystem, ActorSystem> FuncActorSystem { get; set; } = _ => _;
    public Func<RootContext, RootContext> FuncRootContext { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncIRootContext { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncActorSystemStart { get; set; } = _ => _;
    public int SystemShutdownDelaySec { get; set; } = 10;
}
