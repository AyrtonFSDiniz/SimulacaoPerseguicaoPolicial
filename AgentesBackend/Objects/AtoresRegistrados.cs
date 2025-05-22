public class AtoresRegistrados
{
    public SupervisorRef Supervisor { get; }
    public BridgeRef Bridge { get; }

    public AtoresRegistrados(SupervisorRef supervisor, BridgeRef bridge)
    {
        Supervisor = supervisor;
        Bridge = bridge;
    }
}
