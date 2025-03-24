using Akka.Actor;

public class LadraoActor : ReceiveActor
{
    private Random _random = new Random();
    
    public LadraoActor()
    {
        Receive<string>(mensagem =>
        {
            if (mensagem == "mover")
            {
                var novaPosicao = (X: _random.Next(0, 10), Y: _random.Next(0, 10));
                Sender.Tell(novaPosicao);
            }
        });
    }
}