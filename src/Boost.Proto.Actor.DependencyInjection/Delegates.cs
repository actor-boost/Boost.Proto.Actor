namespace Boost.Proto.Actor.DependencyInjection;

public delegate ActorSystem FuncActorSystem(ActorSystem actorSystem);
public delegate ActorSystemConfig FuncActorSystemConfig(ActorSystemConfig actorSystemConfig);
public delegate IRootContext FuncRootContext(IRootContext root);
public delegate IRootContext FuncActorSystemStart(IRootContext root);
