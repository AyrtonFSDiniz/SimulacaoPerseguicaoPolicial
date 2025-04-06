using Akka.Actor;

public class SupervisorActor : ReceiveActor
{
    private int _contadorLadroes = 0;
    private int _contadorPoliciais = 0;
    private readonly Dictionary<string, IActorRef> _atores = new Dictionary<string, IActorRef>();
    private readonly Dictionary<string, (int X, int Y)> _posicoes = new Dictionary<string, (int X, int Y)>();
    private readonly Random _random = new Random();
    private readonly int _raioDeVisao = 5; // Campo de visão em unidades
    private readonly System.Timers.Timer _timer;
    private readonly int _tempoDeMovimentacao = 3000; // Tempo de movimentação em milissegundos


    public SupervisorActor()
    {
        // Timer para movimentação automática
        _timer = new System.Timers.Timer(_tempoDeMovimentacao); // Movimenta a cada 1 segundo
        _timer.Elapsed += (_, _) => MoverAutomatico();
        _timer.Start();

        Receive<string>(msg =>
        {
            if (msg == "criarLadrao")
            {
                var nomeLadrao = $"ladrao-{_contadorLadroes++}";
                var ladraoActor = Context.ActorOf(Props.Create<LadraoActor>(), nomeLadrao);
                _atores[nomeLadrao] = ladraoActor;
                _posicoes[nomeLadrao] = (X: _random.Next(0, 10), Y: _random.Next(0, 10));
                Sender.Tell(nomeLadrao);
            }
            else if (msg == "criarPolicial")
            {
                var nomePolicial = $"policial-{_contadorPoliciais++}";
                var policialActor = Context.ActorOf(Props.Create<PolicialActor>(), nomePolicial);
                _atores[nomePolicial] = policialActor;
                _posicoes[nomePolicial] = (X: _random.Next(0, 10), Y: _random.Next(0, 10));
                Sender.Tell(nomePolicial);
            }
            else if (msg.StartsWith("mover:"))
            {
                var nome = msg.Split(':')[1];
                if (_atores.TryGetValue(nome, out var ator))
                {
                    ConsoleLog.Log($"Movendo ator: {nome}");
                    ator.Ask<(int, int)>("mover")
                        .PipeTo(Sender);
                }
                else
                {
                    ConsoleLog.Log($"Ator {nome} não encontrado.");
                    throw new Exception($"Ator {nome} não encontrado.");
                }
            }
            else if (msg == "obterPosicoes")
            {
                Sender.Tell(_posicoes);
            }
            else
            {
                ConsoleLog.Log($"Mensagem não reconhecida: {msg}");
            }
        });
    }

    
    private void MoverAutomatico()
    {
        
        foreach (var nome in _posicoes.Keys.ToList())
        {
            var (x, y) = _posicoes[nome];

            // Movimentação aleatória
            x += _random.Next(-1, 2);
            y += _random.Next(-1, 2);

            _posicoes[nome] = (x, y);
        }

        VerificarCampoDeVisao();
    }

    private void VerificarCampoDeVisao()
    {
        var ladroes = _posicoes.Where(p => p.Key.StartsWith("ladrao")).ToList();
        var policiais = _posicoes.Where(p => p.Key.StartsWith("policial")).ToList();

        foreach (var (nomeLadrao, (xL, yL)) in ladroes)
        {
            foreach (var (nomePolicial, (xP, yP)) in policiais)
            {
                var distancia = Math.Sqrt(Math.Pow(xP - xL, 2) + Math.Pow(yP - yL, 2));
                if (distancia <= _raioDeVisao)
                {
                    ConsoleLog.Log($"⚠️ {nomePolicial} avistou {nomeLadrao}! Iniciando modo fuga.");

                    // Inicia modo fuga
                    MostrarFugaLog(nomeLadrao, xL, yL, nomePolicial, xP, yP);


                    if (xL == xP && yL == yP) // Verifica se estão na mesma posição
                    {
                        ConsoleLog.Log($"🚔 {nomePolicial} capturou {nomeLadrao}!");

                        // Evento de captura
                        _posicoes.Remove(nomeLadrao); // Remove o ladrão da lista de posições
                        _atores.Remove(nomeLadrao);  // Remove o ladrão do sistema de atores

                        Context.Stop(_atores[nomeLadrao]); // Para o ator ladrão
                        _atores.Remove(nomePolicial); // Remove o policial do sistema de atores

                        Context.Stop(_atores[nomePolicial]); // Para o ator policial
                        ConsoleLog.Log($"🚔 {nomePolicial} capturou {nomeLadrao}!")
                        
                        ;
                    }

                }
            }
        }
    }

    private void MostrarFugaLog(string nomeLadrao, int xL, int yL, string nomePolicial, int xP, int yP)
    {
        _posicoes[nomePolicial] = (xP + Math.Sign(xL - xP), yP + Math.Sign(yL - yP));
        ConsoleLog.Log($"🚓 {nomePolicial} se move para ({_posicoes[nomePolicial].X}, {_posicoes[nomePolicial].Y})");

        _posicoes[nomeLadrao] = (xL - Math.Sign(xL - xP), yL - Math.Sign(yL - yP));
        ConsoleLog.Log($"🏃 {nomeLadrao} se move para ({_posicoes[nomeLadrao].X}, {_posicoes[nomeLadrao].Y})");
    }
}