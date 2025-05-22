using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;

public class SupervisorActor : ReceiveActor
{
    private int _contadorLadroes = 0;
    private int _contadorPoliciais = 0;
    private readonly Dictionary<string, IActorRef> _atores = new();
    private readonly Dictionary<string, (int X, int Y)> _posicoes = new();
    private readonly Random _random = new();
    private readonly int _raioDeVisao = 5;
    private readonly System.Timers.Timer _timer; // Corrigido: System.Timers.Timer
    private readonly int _tempoDeMovimentacao = 3000;
    private readonly IActorRef _bridge;

    public SupervisorActor(IActorRef bridge)
    {
        _bridge = bridge;

        _timer = new System.Timers.Timer(_tempoDeMovimentacao);
        _timer.Elapsed += (_, _) => MoverAutomatico();
        _timer.AutoReset = true;
        _timer.Start();

        Receive<string>(msg =>
        {
            if (msg == "criarLadrao")
            {
                var nomeLadrao = $"ladrao-{_contadorLadroes++}";
                var ladraoActor = Context.ActorOf(Props.Create<LadraoActor>(), nomeLadrao);
                _atores[nomeLadrao] = ladraoActor;
                _posicoes[nomeLadrao] = (_random.Next(0, 10), _random.Next(0, 10));

                _bridge.Tell(new NotificarCriacao(nomeLadrao, "ladrao"));
                Sender.Tell(nomeLadrao);
            }
            else if (msg == "criarPolicial")
            {
                var nomePolicial = $"policial-{_contadorPoliciais++}";
                var policialActor = Context.ActorOf(Props.Create<PolicialActor>(), nomePolicial);
                _atores[nomePolicial] = policialActor;
                _posicoes[nomePolicial] = (_random.Next(0, 10), _random.Next(0, 10));

                _bridge.Tell(new NotificarCriacao(nomePolicial, "policial"));
                Sender.Tell(nomePolicial);
            }
            else if (msg.StartsWith("mover:"))
            {
                var nome = msg.Split(':')[1];
                if (_atores.TryGetValue(nome, out var ator))
                {
                    ConsoleLog.Log($"Movendo ator: {nome}");
                    ator.Ask<(int, int)>("mover").PipeTo(Sender);
                }
                else
                {
                    ConsoleLog.Log($"Ator {nome} não encontrado.");
                    Sender.Tell(new Status.Failure(new Exception($"Ator {nome} não encontrado.")));
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
            x += _random.Next(-1, 2);
            y += _random.Next(-1, 2);
            _posicoes[nome] = (x, y);
        }

        _bridge.Tell(new NotificarAtualizacaoPosicoes(_posicoes));
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
                    _bridge.Tell(new NotificarFuga(nomeLadrao, nomePolicial));

                    MostrarFugaLog(nomeLadrao, xL, yL, nomePolicial, xP, yP);

                    if (_posicoes.TryGetValue(nomeLadrao, out var novaPosLadrao) &&
                        _posicoes.TryGetValue(nomePolicial, out var novaPosPolicial) &&
                        novaPosLadrao == novaPosPolicial)
                    {
                        ConsoleLog.Log($"🚔 {nomePolicial} capturou {nomeLadrao}!");

                        _bridge.Tell(new NotificarCaptura(nomeLadrao, nomePolicial));

                        if (_atores.TryGetValue(nomeLadrao, out var ladrao))
                        {
                            Context.Stop(ladrao);
                            _atores.Remove(nomeLadrao);
                            _posicoes.Remove(nomeLadrao);
                        }

                        if (_atores.TryGetValue(nomePolicial, out var policial))
                        {
                            Context.Stop(policial);
                            _atores.Remove(nomePolicial);
                            _posicoes.Remove(nomePolicial);
                        }
                    }
                }
            }
        }
    }

    private void MostrarFugaLog(string nomeLadrao, int xL, int yL, string nomePolicial, int xP, int yP)
    {
        var novoXPolicial = xP + Math.Sign(xL - xP);
        var novoYPolicial = yP + Math.Sign(yL - yP);
        _posicoes[nomePolicial] = (novoXPolicial, novoYPolicial);
        ConsoleLog.Log($"🚓 {nomePolicial} se move para ({novoXPolicial}, {novoYPolicial})");

        var novoXLadrao = xL - Math.Sign(xL - xP);
        var novoYLadrao = yL - Math.Sign(yL - yP);
        _posicoes[nomeLadrao] = (novoXLadrao, novoYLadrao);
        ConsoleLog.Log($"🏃 {nomeLadrao} se move para ({novoXLadrao}, {novoYLadrao})");
    }
}
