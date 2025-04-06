using Akka.Actor;
using Microsoft.AspNetCore.SignalR;

public class JogoHub : Hub
{
    private readonly IActorRef _supervisor;

    public JogoHub(IActorRef supervisor)
    {
        _supervisor = supervisor;
    }

    public async Task CriarLadrao()
    {
        var nomeLadrao = await _supervisor.Ask<string>("criarLadrao");
        ConsoleLog.Log($"Ladrão criado: {nomeLadrao}");
        await Clients.All.SendAsync("LadraoCriado", nomeLadrao);
    }

    public async Task CriarPolicial()
    {
        var nomePolicial = await _supervisor.Ask<string>("criarPolicial");
        ConsoleLog.Log($"Policial criado: {nomePolicial}");
        await Clients.All.SendAsync("PolicialCriado", nomePolicial);
    }

    public async Task AtualizarPosicoes()
    {
        try
        {
            // Solicita as posições de todos os atores ao supervisor
            var posicoes = await _supervisor.Ask<Dictionary<string, (int X, int Y)>>("obterPosicoes");
            ConsoleLog.Log("Atualizando posições dos atores.");

            foreach (var (nome, posicao) in posicoes)
            {
                ConsoleLog.Log($"Ator: {nome}, Posição: {posicao}");
                await Clients.All.SendAsync("AtualizarPosicao", nome, posicao);
            }
        }
        catch (Exception ex)
        {
            ConsoleLog.Log($"Erro ao atualizar posições: {ex.Message}");
        }
    }

    public async Task MoverLadrao(string nomeLadrao)
    {
        if (string.IsNullOrWhiteSpace(nomeLadrao))
        {
            throw new ArgumentException("O nome do ladrão não pode ser vazio ou nulo.");
        }

        try
        {
            // Envie uma mensagem ao supervisor para mover um ladrão específico
            var posicao = await _supervisor.Ask<(int, int)>($"mover:{nomeLadrao}");
            ConsoleLog.Log($"Ladrão {nomeLadrao} movido para a posição: {posicao}");
            await Clients.All.SendAsync("AtualizarPosicaoLadrao", nomeLadrao, posicao);
        }
        catch (Exception ex)
        {
            ConsoleLog.Log($"Erro ao mover ladrão {nomeLadrao}: {ex.Message}");
            throw new HubException("Falha ao executar a movimentação do ladrão no servidor.");
        }
    }

    public async Task MoverPolicial(string nomePolicial)
    {
        if (string.IsNullOrWhiteSpace(nomePolicial))
        {
            throw new ArgumentException("O nome do policial não pode ser vazio ou nulo.");
        }

        try
        {
            // Envie uma mensagem ao supervisor para mover um policial específico
            var posicao = await _supervisor.Ask<(int, int)>($"mover:{nomePolicial}");
            ConsoleLog.Log($"Policial {nomePolicial} movido para a posição: {posicao}");
            await Clients.All.SendAsync("AtualizarPosicaoPolicial", nomePolicial, posicao);
        }
        catch (Exception ex)
        {
            ConsoleLog.Log($"Erro ao mover policial {nomePolicial}: {ex.Message}");
            throw new HubException("Falha ao executar a movimentação do policial no servidor.");
        }
    }

    public async Task ModoFuga(string nomeLadrao, string nomePolicial)
    {
        ConsoleLog.Log($"Ativando modo fuga: Ladrão: {nomeLadrao}, Policial: {nomePolicial}");
        await Clients.All.SendAsync("ModoFugaIniciado", nomeLadrao, nomePolicial);
    }

    public async Task Perseguir(string nomePolicial)
    {
        // Envie uma mensagem ao supervisor para que um policial específico comece a perseguir
        _supervisor.Tell($"perseguir:{nomePolicial}");
        ConsoleLog.Log($"Policial {nomePolicial} está perseguindo.");
        await Clients.All.SendAsync("PolicialPerseguindo", nomePolicial);
    }

    public async Task LadraoCapturado(string nomeLadrao, string nomePolicial)
    {
        await Clients.All.SendAsync("LadraoCapturado", nomeLadrao, nomePolicial);
    }

}