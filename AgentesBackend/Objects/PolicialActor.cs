using Akka.Actor;

public class PolicialActor : ReceiveActor
{
    public PolicialActor()
    {
        Receive<string>(mensagem =>
        {
            if (mensagem == "perseguir")
            {
                ConsoleLog.Log("O policial está perseguindo o ladrão!");
            }
            else if (mensagem == "reforço")
            {
                ConsoleLog.Log("O policial chamou reforço!");
            }else{
                ConsoleLog.Log($"Mensagem não reconhecida: {mensagem}");
            }
        });
    }
}