using System.Threading;
using System.Threading.Tasks;

public abstract class PlayerConnection
{
    private static int nextConnectorDescriptor;

    private HumanPlayer player;
    private int connectorDescriptor;

    public Dbref Dbref => player.id;

    public string Name => player.name;

    public Dbref Location => player.location;

    public int ConnectorDescriptor => connectorDescriptor;

    protected PlayerConnection(HumanPlayer player)
    {
        this.player = player;
        var next = Interlocked.Increment(ref nextConnectorDescriptor);
        if (next > int.MaxValue - 100000)
        {
            nextConnectorDescriptor = 1;
            next = 1;
        }
        this.connectorDescriptor = next;
    }

    public abstract void receiveInput(string input);

    public abstract Task<CommandResult> popCommand();

    public abstract Task sendOutput(string output);

    public async Task<Dbref> MatchAsync(string name, CancellationToken cancellationToken)
    {
        return await this.player.MatchAsync(name, cancellationToken);
    }
}