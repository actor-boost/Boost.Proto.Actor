module Tests

open System
open Xunit
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Proto
open Boost.Proto.Actor.Hosting.Cluster
open Proto.Cluster
open Boost.Proto.Actor.DependencyInjection

type TestActorMessage =
| Hello
| World

type TestActor(TestActorFactory : IPropsFactory<TestActor>) =
    interface IActor with
        member _.ReceiveAsync(context) = task {
            match context.Message with
            | :? TestActorMessage as msg ->
                match msg with
                | Hello -> context.Respond(World)
                | World -> context.Respond(Hello)
            | _ -> ()
        }

let host = Host.CreateDefaultBuilder()
               .UseProtoActorCluster(Action<_,_>(fun opt -> fun sp ->
                   opt.Name <- "OverrideClusterName1"
                   opt.ClusterKinds.Add(
                       ClusterKind("TestActor", sp.GetRequiredService<IPropsFactory<TestActor>>().Create()))))
               .Build();

host.StartAsync() |> Async.AwaitTask |> Async.RunSynchronously

[<Fact>]
let ``My test`` () =
    let root = host.Services.GetRequiredService<IRootContext>()

    let ret = Async.RunSynchronously <| async {
        let! ct = Async.CancellationToken
        return! root.System.Cluster().RequestAsync<TestActorMessage>("1", "TestActor", Hello, ct) |> Async.AwaitTask
    } 

    Assert.Equal(World, ret)

