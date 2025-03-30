
using Akka.Actor;
using Microsoft.AspNetCore.SignalR;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR();

var system = ActorSystem.Create("AgentesSistema");
var ladraoActor = system.ActorOf(Props.Create<LadraoActor>(), "ladrao");
var policialActor = system.ActorOf(Props.Create<PolicialActor>(), "policial");

builder.Services.AddSingleton(system);
builder.Services.AddSingleton<IActorRef>(ladraoActor);
builder.Services.AddSingleton<IActorRef>(policialActor);

var supervisor = system.ActorOf(Props.Create<SupervisorActor>(), "supervisor");
builder.Services.AddSingleton<IActorRef>(supervisor);


builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins("http://localhost:5172")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();

app.UseCors("CorsPolicy");

app.UseRouting();

app.MapHub<JogoHub>("/jogoHub", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                         Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
});


app.Run();