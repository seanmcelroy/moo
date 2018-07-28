using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class Look : Action
{
    public sealed override bool canProcess(Player player, CommandResult command)
    {
        string verb = command.getVerb().ToLowerInvariant();
        return (verb == "l" || verb == "look");
    }

    public sealed override async Task<VerbResult> process(Player player, CommandResult command, CancellationToken cancellationToken)
    {
        if ((!command.hasDirectObject() && !command.hasIndirectObject())
            || (command.hasDirectObject() && string.Compare("here", command.getDirectObject()) == 0))
        {
            await lookAtRoom(player, player.location, cancellationToken);
            return new VerbResult(true, "");
        }

        await player.sendOutput("You don't see that here.");
        return new VerbResult(false, "No reference");
    }

    private async Task lookAtRoom(Player player, int locationId, CancellationToken cancellationToken)
    {
        GetResult<Container> locationLookup = await ThingRepository.GetAsync<Container>(locationId, cancellationToken);
        if (locationLookup.isSuccess)
        {
            Container location = locationLookup.value;
            await player.sendOutput($"{location.name}(#{location.id})");
            await player.sendOutput(location.internalDescription);

            StringBuilder otherHumans = new StringBuilder();
            otherHumans.Append("You see ");
            int count = 0;
            foreach (HumanPlayer peer in location.GetVisibleHumanPlayersForAsync(player, cancellationToken))
            {
                otherHumans.Append(count == 0 ? peer.name : ", " + peer.name);
                count++;
            }
            if (count > 0)
                await player.sendOutput(otherHumans.AppendFormat(" here.").ToString());
        }
        else
        {
            await player.sendOutput($"You see nothing {(player.location == locationId ? "here" : "there")}.");
        }
    }
}