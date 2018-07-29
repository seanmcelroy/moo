using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class @Load : Action
{
    public sealed override Tuple<bool, string> CanProcess(Player player, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@load" && command.hasDirectObject())
            return new Tuple<bool, string>(true, verb);
        return new Tuple<bool, string>(false, null);
    }

    public override ActionType Type => ActionType.BuiltIn;

    public sealed override async Task<VerbResult> Process(Player player, CommandResult command, CancellationToken cancellationToken)
    {
        var targetId = command.resolveDirectObject(player);
        if (targetId == Dbref.NOT_FOUND)
        {
            await player.sendOutput("I don't see that here.");
            return new VerbResult(false, "Target not found");
        }

        if (targetId == Dbref.AMBIGUOUS)
        {
            await player.sendOutput("I don't know which one you mean.");
            return new VerbResult(false, "Multiple targets found");
        }

        var lookup = await ThingRepository.GetAsync<Thing>(targetId, cancellationToken);
        if (lookup.isSuccess)
        {
            var target = lookup.value;
            var loadResult = await ThingRepository.LoadFromDatabaseAsync<Thing>(targetId, cancellationToken);
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