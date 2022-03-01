using Boost.Proto.Actor.BlazorWasm;
using Boost.Proto.Actor.BlazorWasm.SampleApp;
using Boost.Proto.Actor.BlazorWasm.SampleApp.Actors.Counter;
using Boost.Proto.Actor.DependencyInjection;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.UseProtoActor((sp, option) =>
{
    option.FuncActorSystemStart = root =>
    {
        root.SpawnNamed(sp.GetRequiredService<IPropsFactory<CounterActor>>()
                          .Create(new CounterState(10)), nameof(CounterActor));

        return root;
    };
});

var app = builder.Build();

app.RunProtoActor();

await app.RunAsync();



