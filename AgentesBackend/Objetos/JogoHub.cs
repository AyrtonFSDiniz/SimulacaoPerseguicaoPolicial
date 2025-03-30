using Akka.Actor;
using Microsoft.AspNetCore.SignalR;

public class JogoHub : Hub
{
    private readonly IActorRef _supervisor;
    private readonly ILogger<JogoHub> _logger;

    public JogoHub(IActorRef supervisor, ILogger<JogoHub> logger)
    {
        _logger = logger;
        _logger.LogInformation("JogoHub criado.");
        _supervisor = supervisor;
    }

    public async Task CriarLadrao()
    {
        var nomeLadrao = await _supervisor.Ask<string>("criarLadrao");
        _logger.LogInformation($"Ladrão criado: {nomeLadrao}");
        await Clients.All.SendAsync("LadraoCriado", nomeLadrao);
    }

    public async Task CriarPolicial()
    {
        var nomePolicial = await _supervisor.Ask<string>("criarPolicial");
        _logger.LogInformation($"Policial criado: {nomePolicial}");
        await Clients.All.SendAsync("PolicialCriado", nomePolicial);
    }

    public async Task AtualizarPosicoes()
    {
        try
        {
            // Solicita as posições de todos os atores ao supervisor
            var posicoes = await _supervisor.Ask<Dictionary<string, (int X, int Y)>>("obterPosicoes");
            _logger.LogInformation("Atualizando posições dos atores.");

            foreach (var (nome, posicao) in posicoes)
            {
                _logger.LogInformation($"Ator: {nome}, Posição: {posicao}");
                await Clients.All.SendAsync("AtualizarPosicao", nome, posicao);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao atualizar posições: {ex.Message}");
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
            _logger.LogInformation($"Ladrão {nomeLadrao} movido para a posição: {posicao}");
            await Clients.All.SendAsync("AtualizarPosicaoLadrao", nomeLadrao, posicao);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao mover ladrão {nomeLadrao}: {ex.Message}");
            throw new HubException("Falha ao executar a movimentação do ladrão no servidor.");
        }
    }

    public async Task ModoFuga(string nomeLadrao, string nomePolicial)
    {
        _logger.LogInformation($"Ativando modo fuga: Ladrão: {nomeLadrao}, Policial: {nomePolicial}");
        await Clients.All.SendAsync("ModoFugaIniciado", nomeLadrao, nomePolicial);
    }

    public async Task Perseguir(string nomePolicial)
    {
        // Envie uma mensagem ao supervisor para que um policial específico comece a perseguir
        _supervisor.Tell($"perseguir:{nomePolicial}");
        _logger.LogInformation($"Policial {nomePolicial} está perseguindo.");
        await Clients.All.SendAsync("PolicialPerseguindo", nomePolicial);
    }
}