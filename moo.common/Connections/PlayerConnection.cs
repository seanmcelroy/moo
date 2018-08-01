using System.Threading;
using System.Threading.Tasks;

public abstract class PlayerConnection
{
    private HumanPlayer player;

    public Dbref Dbref => player.id;

    public string Name => player.name;

    public Dbref Location => player.location;

    protected PlayerConnection(HumanPlayer player)
    {
        this.player = player;
    }

    public abstract void receiveInput(string input);

    public abstract Task<CommandResult> popCommand();

    public abstract Task sendOutput(string output);

    public async Task<Dbref> MatchAsync(string name, CancellationToken cancellationToken)
    {
        return await this.player.MatchAsync(name, cancellationToken);
    }
}