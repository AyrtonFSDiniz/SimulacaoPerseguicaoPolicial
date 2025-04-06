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
                ConsoleLog.Log($"Ladrão movido para: {novaPosicao}");

                Sender.Tell(novaPosicao);
            }
            else if (mensagem == "esconder")
            {
                ConsoleLog.Log("Ladrão se escondeu!");
            }
            else if (mensagem == "roubar")
            {
                ConsoleLog.Log("Ladrão está roubando!");
            }
            else if (mensagem == "fugir")
            {
                ConsoleLog.Log("Ladrão está fugindo!");
            }
            else if (mensagem == "capturado")
            {
                ConsoleLog.Log("Ladrão foi capturado!");
                Context.Stop(Self); // Para o ator se ele for capturado
            }
            else
            {
                ConsoleLog.Log($"Mensagem não reconhecida: {mensagem}");
                Unhandled(mensagem); // Mensagem não tratada

            }
        });
    }
}