using Akka.Actor;

public class PolicialActor : ReceiveActor
{
    public PolicialActor()
    {
        Receive<string>(mensagem =>
        {
            if (mensagem == "perseguir")
            {
                Console.WriteLine("O policial está perseguindo o ladrão!");
            }
            else if (mensagem == "reforço")
            {
                Console.WriteLine("O policial chamou reforço!");
            }
        });
    }
}