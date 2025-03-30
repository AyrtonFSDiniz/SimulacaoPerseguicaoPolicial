using System.Timers;
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

    public SupervisorActor()
    {
        // Timer para movimentação automática
        _timer = new System.Timers.Timer(1000); // Movimenta a cada 1 segundo
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
                    Console.WriteLine($"Movendo ator: {nome}");
                    ator.Ask<(int, int)>("mover")
                        .PipeTo(Sender);
                }
                else
                {
                    Console.WriteLine($"Ator {nome} não encontrado.");
                    throw new Exception($"Ator {nome} não encontrado.");
                }
            }
            else if (msg == "obterPosicoes")
            {
                Sender.Tell(_posicoes);
            }
            else
            {
                Console.WriteLine($"Mensagem não reconhecida: {msg}");
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
                    Console.WriteLine($"⚠️ {nomePolicial} avistou {nomeLadrao}! Iniciando modo fuga.");

                    // Inicia modo fuga
                    _posicoes[nomePolicial] = (xP + Math.Sign(xL - xP), yP + Math.Sign(yL - yP));
                    _posicoes[nomeLadrao] = (xL - Math.Sign(xL - xP), yL - Math.Sign(yL - yP));
                }
            }
        }
    }
}