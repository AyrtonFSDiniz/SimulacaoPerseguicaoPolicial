using Akka.Actor;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = WebApplication.CreateBuilder(args);

// Serviços
builder.Services.AddSignalR();

// Akka.NET Actor System
var actorSystem = ActorSystem.Create("AgentesSistema");
builder.Services.AddSingleton(actorSystem);

// Registra atores em classe wrapper
builder.Services.AddSingleton(sp =>
{
    var hubContext = sp.GetRequiredService<IHubContext<JogoHub>>();
    var bridge = actorSystem.ActorOf(Props.Create(() => new SignalRBridgeActor(hubContext)), "bridge");
    var supervisor = actorSystem.ActorOf(Props.Create(() => new SupervisorActor(bridge)), "supervisor");

    return new AtoresRegistrados(new SupervisorRef(supervisor), new BridgeRef(bridge));
});

builder.Services.AddSingleton<SupervisorRef>(sp => sp.GetRequiredService<AtoresRegistrados>().Supervisor);
builder.Services.AddSingleton<BridgeRef>(sp => sp.GetRequiredService<AtoresRegistrados>().Bridge);

// CORS
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

// Hub SignalR
app.MapHub<JogoHub>("/jogoHub", options =>
{
    options.Transports = Microsoft.AspNetCore.Http.Connections.HttpTransportType.WebSockets |
                         Microsoft.AspNetCore.Http.Connections.HttpTransportType.ServerSentEvents;
});

app.Run();