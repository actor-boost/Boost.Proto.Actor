using System.Threading.Tasks;

namespace Boost.Proto.Actor.DependencyInjection;

public delegate ActorSystem FuncActorSystem(ActorSystem actorSystem);
public delegate ActorSystemConfig FuncActorSystemConfig(ActorSystemConfig actorSystemConfig);
public delegate IRootContext FuncIRootContext(IRootContext root);
public delegate IRootContext FuncActorSystemStart(IRootContext root);
public delegate Props FuncProps(Props props);
