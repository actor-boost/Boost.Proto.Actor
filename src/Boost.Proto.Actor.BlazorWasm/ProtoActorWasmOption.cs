using Microsoft.Extensions.Configuration;
using Proto;

namespace Boost.Proto.Actor.BlazorWasm;

public class ProtoActorWasmOption
{
    public ProtoActorWasmOption(IConfiguration configuration) => Configuration = configuration;

    public IConfiguration Configuration { get; }

    public Func<ActorSystemConfig, ActorSystemConfig> FuncActorSystemConfig { get; set; } = _ => _;

    public Func<ActorSystem, ActorSystem> FuncActorSystem { get; set; } = _ => _;

    public Func<RootContext, RootContext> FuncRootContext { get; set; } = _ => _;

    public Func<IRootContext, IRootContext> FuncIRootContext { get; set; } = _ => _;

    public Func<IRootContext, Func<IRootContext, Task>, Task> FuncActorSystemStartAsync { get; set; } = (r, n) => n(r);


}
