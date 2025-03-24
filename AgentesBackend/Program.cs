
using Akka.Actor;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var system = ActorSystem.Create("AgentesSistema");
var ladraoActor = system.ActorOf(Props.Create<LadraoActor>(), "ladrao");
var policialActor = system.ActorOf(Props.Create<PolicialActor>(), "policial");

builder.Services.AddSingleton(system);
builder.Services.AddSingleton<IActorRef>(ladraoActor);
builder.Services.AddSingleton<IActorRef>(policialActor);

var app = builder.Build();

app.UseRouting();

app.UseEndpoints(endpoints =>
{
    endpoints.MapHub<JogoHub>("/jogoHub");
});

app.Run();