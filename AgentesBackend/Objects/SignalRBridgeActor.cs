using Akka.Actor;
using Microsoft.AspNetCore.SignalR;

public class SignalRBridgeActor : ReceiveActor
{
    private readonly IHubContext<JogoHub> _hubContext;

    public SignalRBridgeActor(IHubContext<JogoHub> hubContext)
    {
        _hubContext = hubContext;

        Receive<NotificarCriacao>(msg =>
        {
            _hubContext.Clients.All
                .SendAsync("AtorCriado", msg.Nome, msg.Tipo)
                .PipeTo(Self, Sender);
        });

        Receive<NotificarAtualizacaoPosicoes>(msg =>
        {
            var posicoesFormatadas = msg.Posicoes
                .Select(p => new
                {
                    Id = p.Key,
                    X = p.Value.X,
                    Y = p.Value.Y
                }).ToList();

            _hubContext.Clients.All
                .SendAsync("AtualizarTodasPosicoes", posicoesFormatadas)
                .PipeTo(Self, Sender);
        });

        Receive<NotificarCaptura>(msg =>
        {
            _hubContext.Clients.All
                .SendAsync("LadraoCapturado", msg.NomeLadrao, msg.NomePolicial)
                .PipeTo(Self, Sender);
        });

        Receive<NotificarFuga>(msg =>
        {
            _hubContext.Clients.All
                .SendAsync("ModoFugaIniciado", msg.NomeLadrao, msg.NomePolicial)
                .PipeTo(Self, Sender);
        });

        // Opcional: lidar com sucesso ou falha do SendAsync
        Receive<Task>(task =>
        {
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    ConsoleLog.Log($"Erro ao enviar notificação SignalR: {t.Exception?.GetBaseException().Message}");
                }
            });
        });
    }
}
