using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class Look : IRunnable
{
    public Tuple<bool, string?> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (string.Compare(verb, "l", StringComparison.OrdinalIgnoreCase) == 0 && command.hasDirectObject())
            return new Tuple<bool, string?>(true, verb);
        return new Tuple<bool, string?>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        if ((!command.hasDirectObject() && !command.hasIndirectObject())
            || (command.hasDirectObject() && string.Compare("here", command.getDirectObject()) == 0))
        {
            await lookAtRoom(connection, connection.Location, cancellationToken);
            return new VerbResult(true, "");
        }

        await connection.sendOutput("You don't see that here.");
        return new VerbResult(false, "No reference");
    }

    private async Task lookAtRoom(PlayerConnection connection, Dbref locationId, CancellationToken cancellationToken)
    {
        var locationLookup = await ThingRepository.GetAsync<Container>(locationId, cancellationToken);
        if (locationLookup.isSuccess && locationLookup.value != null)
        {
            var location = locationLookup.value;
            await connection.sendOutput(location.UnparseObject());
            if (location.internalDescription != null)
                await connection.sendOutput(location.internalDescription);

            var otherHumans = new StringBuilder();
            otherHumans.Append("You see ");
            int count = 0;
            foreach (var peer in location.GetVisibleHumanPlayersForAsync(cancellationToken))
            {
                otherHumans.Append(count == 0 ? peer.name : ", " + peer.name);
                count++;
            }
            if (count > 0)
                await connection.sendOutput(otherHumans.Append(" here.").ToString());
        }
        else
        {
            await connection.sendOutput($"You see nothing {(connection.Location == locationId ? "here" : "there")}.");
        }
    }
}