using Microsoft.AspNetCore.SignalR;

public class JogoService : IJogoService
{
    private readonly IHubContext<JogoHub> _hubContext;

    public JogoService(IHubContext<JogoHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task EnviarAtualizacoes(Dictionary<string, (int X, int Y)> posicoes)
    {
        foreach (var (nome, posicao) in posicoes)
        {
            await _hubContext.Clients.All.SendAsync("AtualizarPosicao", nome, posicao);
        }
    }
}