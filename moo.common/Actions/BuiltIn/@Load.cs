using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class @Load : Action
{
    public sealed override bool CanProcess(Player player, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        return (verb == "@load" && command.hasDirectObject());
    }

    public override ActionType Type => ActionType.BuiltIn;

    public sealed override async Task<VerbResult> Process(Player player, CommandResult command, CancellationToken cancellationToken)
    {
        var targetId = command.resolveDirectObject(player);
        if (targetId == null)
        {
            await player.sendOutput("I don't see that here.");
            return new VerbResult(false, "Target not found");
        }

        var lookup = await ThingRepository.GetAsync<Thing>(targetId.Value, cancellationToken);
        if (lookup.isSuccess)
        {
            var target = lookup.value;
            var loadResult = await ThingRepository.LoadFromDatabaseAsync<Thing>(targetId.Value, cancellationToken);
            await player.sendOutput($"Load from database: {loadResult.isSuccess}");
        }
        else
        {
            await player.sendOutput("You can't seem to find that.");
            return new VerbResult(false, "Target not found");
        }

        return new VerbResult(true, "");
    }
}