using Boost.Proto.Actor.DependencyInjection;
using Boost.Proto.Actor.Hosting.Local;
using Boost.Proto.Actor.Hosting.Local.SampleApp.Actors;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Host.UseProtoActor((sp, option) =>
{
    option.ActorSystemStart = root =>
    {
        root.SpawnNamed(sp.GetService<IPropsFactory<CounterActor>>()
                          .Create(), nameof(CounterActor));
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();

app.MapControllers();

app.MapGet("/Counter", async (IRootContext root) =>
{
    var pid = new PID(root.System.Address, nameof(CounterActor));
    return await root.RequestAsync<int>(pid, 1);
});

app.Run();
