using System.Threading.Tasks;

namespace Boost.Proto.Actor.DependencyInjection;

public delegate ActorSystem FuncActorSystem(ActorSystem actorSystem);
public delegate ActorSystemConfig FuncActorSystemConfig(ActorSystemConfig actorSystemConfig);
public delegate IRootContext FuncIRootContext(IRootContext root);
public delegate Task FuncActorSystemStartAsync(IRootContext root, Func<IRootContext, Task> next);
public delegate Props FuncProps(Props props);
