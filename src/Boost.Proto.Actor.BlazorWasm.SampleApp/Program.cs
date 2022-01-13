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
    option.ActorSystemStart = root =>
    {
        root.SpawnNamed(sp.GetService<IPropsFactory<CounterActor>>()
                          .Create(new CounterState(0)), nameof(CounterActor));
    };
});

var app = builder.Build();

await app.RunAsync();



