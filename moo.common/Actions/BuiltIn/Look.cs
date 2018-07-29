using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class Look : Action
{
    public override ActionType Type => ActionType.BuiltIn;

    public Look() {
        this.aliases.Add("l");
    }

    public sealed override async Task<VerbResult> Process(Player player, CommandResult command, CancellationToken cancellationToken)
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
        var locationLookup = await ThingRepository.GetAsync<Container>(locationId, cancellationToken);
        if (locationLookup.isSuccess)
        {
            var location = locationLookup.value;
            await player.sendOutput($"{location.name}(#{location.id})");
            await player.sendOutput(location.internalDescription);

            var otherHumans = new StringBuilder();
            otherHumans.Append("You see ");
            int count = 0;
            foreach (var peer in location.GetVisibleHumanPlayersForAsync(player, cancellationToken))
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