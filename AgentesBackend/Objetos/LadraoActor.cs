using Akka.Actor;

public class LadraoActor : ReceiveActor
{
    private Random _random = new Random();
    private int _x = 0;
    private int _y = 0;


    public LadraoActor()
    {
        Receive<string>(mensagem =>
        {
            if (mensagem == "mover")
            {
                _x += _random.Next(1, 3); // Incremento aleatório
                _y += _random.Next(1, 3);
                var novaPosicao = (_x, _y);
                Console.WriteLine($"Ladrão movido para: {novaPosicao}");

                Sender.Tell(novaPosicao);
            }
            else
            {
                Console.WriteLine($"Mensagem não reconhecida: {mensagem}");
                Unhandled(mensagem); // Mensagem não tratada

            }
        });
    }
}