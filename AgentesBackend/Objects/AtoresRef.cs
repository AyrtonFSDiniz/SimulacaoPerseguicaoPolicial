using Akka.Actor;

public class SupervisorRef
{
    public IActorRef Ref { get; }

    public SupervisorRef(IActorRef actor)
    {
        Ref = actor;
    }
}

public class BridgeRef
{
    public IActorRef Actor { get; }

    public BridgeRef(IActorRef actor)
    {
        Actor = actor;
    }
}
