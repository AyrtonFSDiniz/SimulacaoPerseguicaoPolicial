using Akka.Actor;
using Microsoft.AspNetCore.SignalR;

public class JogoHub : Hub
{
    private readonly IActorRef _ladrao;
    private readonly IActorRef _policial;

    public JogoHub(ActorSystem sistema)
    {
        _ladrao = sistema.ActorOf<LadraoActor>("ladrao");
        _policial = sistema.ActorOf<PolicialActor>("policial");
    }

    public async Task MoverLadrao()
    {
        var posicao = await _ladrao.Ask<(int, int)>("mover");
        await Clients.All.SendAsync("AtualizarPosicaoLadrao", posicao);
    }

    public async Task Perseguir()
    {
        _policial.Tell("perseguir");
        await Clients.All.SendAsync("PolicialPerseguindo");
    }
}