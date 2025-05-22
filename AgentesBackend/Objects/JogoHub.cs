using Akka.Actor;
using Microsoft.AspNetCore.SignalR;

public class Posicao
{
    public int X { get; set; }
    public int Y { get; set; }
}

public class JogoHub : Hub
{
    private readonly IActorRef _supervisor;

    public JogoHub(SupervisorRef supervisor)
    {
        _supervisor = supervisor.Ref;
    }

    public async Task CriarLadrao()
    {
        var nomeLadrao = await _supervisor.Ask<string>("criarLadrao");
        ConsoleLog.Log($"Ladrão criado: {nomeLadrao}");
        await Clients.All.SendAsync("LadraoCriado", nomeLadrao);
        await AtualizarPosicoes();
    }

    public async Task CriarPolicial()
    {
        var nomePolicial = await _supervisor.Ask<string>("criarPolicial");
        ConsoleLog.Log($"Policial criado: {nomePolicial}");
        await Clients.All.SendAsync("PolicialCriado", nomePolicial);
        await AtualizarPosicoes();
    }

    public async Task AtualizarPosicoes()
    {
        try
        {
            var posicoes = await _supervisor.Ask<Dictionary<string, (int X, int Y)>>("obterPosicoes");
            ConsoleLog.Log("Atualizando posições dos atores.");

            // Use Task.WhenAll para executar em paralelo as notificações
            var tasks = posicoes.Select(kvp =>
                Clients.All.SendAsync("AtualizarPosicao", kvp.Key, new Posicao { X = kvp.Value.X, Y = kvp.Value.Y })
            );

            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            ConsoleLog.Log($"Erro ao atualizar posições: {ex.Message}");
        }
    }

    public async Task MoverLadrao(string nomeLadrao)
    {
        if (string.IsNullOrWhiteSpace(nomeLadrao))
            throw new ArgumentException("O nome do ladrão não pode ser vazio ou nulo.");

        try
        {
            var posicaoTuple = await _supervisor.Ask<(int, int)>($"mover:{nomeLadrao}");
            var posicao = new Posicao { X = posicaoTuple.Item1, Y = posicaoTuple.Item2 };

            ConsoleLog.Log($"Ladrão {nomeLadrao} movido para a posição: ({posicao.X},{posicao.Y})");
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
            throw new ArgumentException("O nome do policial não pode ser vazio ou nulo.");

        try
        {
            var posicaoTuple = await _supervisor.Ask<(int, int)>($"mover:{nomePolicial}");
            var posicao = new Posicao { X = posicaoTuple.Item1, Y = posicaoTuple.Item2 };

            ConsoleLog.Log($"Policial {nomePolicial} movido para a posição: ({posicao.X},{posicao.Y})");
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
        _supervisor.Tell($"perseguir:{nomePolicial}");
        ConsoleLog.Log($"Policial {nomePolicial} está perseguindo.");
        await Clients.All.SendAsync("PolicialPerseguindo", nomePolicial);
    }

    public async Task LadraoCapturado(string nomeLadrao, string nomePolicial)
    {
        await Clients.All.SendAsync("LadraoCapturado", nomeLadrao, nomePolicial);
    }
}
