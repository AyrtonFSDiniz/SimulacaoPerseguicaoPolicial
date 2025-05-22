public interface IJogoService
{
    Task EnviarAtualizacoes(Dictionary<string, (int X, int Y)> posicoes);
}