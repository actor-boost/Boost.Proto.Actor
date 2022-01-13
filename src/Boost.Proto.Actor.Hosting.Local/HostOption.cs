using Microsoft.Extensions.Configuration;
using Proto;

namespace Boost.Proto.Actor.Hosting.Local;

public class HostOption
{
    public HostOption(IConfiguration configuration) => Configuration = configuration;
    public IConfiguration Configuration { get; }
    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; } = _ => _;
    public Func<ActorSystem, ActorSystem> FuncActorSystem { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncRootContext { get; set; } = _ => _;
    public Func<IRootContext, IRootContext> FuncActorSystemStart { get; set; } = _ => _;
}
