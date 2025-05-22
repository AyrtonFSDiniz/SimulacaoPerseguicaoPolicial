public enum EventoAleatorioTipo
{
    Chuva,
    Neblina,
    FalhaComunicacao,
    Nada
}

public class EventoAleatorio
{
    public EventoAleatorioTipo Tipo { get; set; }
    public int DuracaoSegundos { get; set; }
    public DateTime Inicio { get; set; }
}
