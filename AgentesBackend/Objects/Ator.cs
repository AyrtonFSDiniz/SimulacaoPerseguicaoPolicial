public class Ator
{
    public string Nome { get; set; }
    public string Tipo { get; set; } // "Ladrao" ou "Policial"
    public int Velocidade { get; set; }
    public int AlcanceVisao { get; set; } // Para policiais
    public int PoderEsconderijo { get; set; } // Para ladrões
    public (int X, int Y) Posicao { get; set; }

    public int EnergiaMaxima { get; set; } = 100;
    public int EnergiaAtual { get; set; } = 100;

    public void GastarEnergia(int quantidade)
    {
        EnergiaAtual = Math.Max(0, EnergiaAtual - quantidade);
    }

    public void RecuperarEnergia(int quantidade)
    {
        EnergiaAtual = Math.Min(EnergiaMaxima, EnergiaAtual + quantidade);
    }

    public bool EstaExausto() => EnergiaAtual == 0;
}
