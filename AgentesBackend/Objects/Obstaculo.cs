public class Obstaculo
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Largura { get; set; }
    public int Altura { get; set; }

    // Verifica se uma posição está dentro do obstáculo
    public bool ContemPosicao(int x, int y)
    {
        return x >= X && x < X + Largura && y >= Y && y < Y + Altura;
    }
}
