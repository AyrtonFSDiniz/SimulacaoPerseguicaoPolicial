public record NotificarCriacao(string Nome, string Tipo); // Tipo = "ladrao" ou "policial"
public record NotificarAtualizacaoPosicoes(Dictionary<string, (int X, int Y)> Posicoes);
public record NotificarCaptura(string NomeLadrao, string NomePolicial);
public record NotificarFuga(string NomeLadrao, string NomePolicial);
