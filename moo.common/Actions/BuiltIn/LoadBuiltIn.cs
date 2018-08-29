using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class LoadBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@load" && command.hasDirectObject())
            return new Tuple<bool, string>(true, verb);
        return new Tuple<bool, string>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        var targetId = await command.ResolveDirectObject(connection.GetPlayer(), cancellationToken);
        if (targetId == Dbref.NOT_FOUND)
        {
            await connection.sendOutput("I don't see that here.");
            return new VerbResult(false, "Target not found");
        }

        if (targetId == Dbref.AMBIGUOUS)
        {
            await connection.sendOutput("I don't know which one you mean.");
            return new VerbResult(false, "Multiple targets found");
        }

        var lookup = await ThingRepository.GetAsync<Thing>(targetId, cancellationToken);
        if (lookup.isSuccess)
        {
            var target = lookup.value;
            var loadResult = await ThingRepository.LoadFromDatabaseAsync<Thing>(targetId, cancellationToken);
            await connection.sendOutput($"Load from database: {loadResult.isSuccess}");
        }
        else
        {
            await connection.sendOutput("You can't seem to find that.");
            return new VerbResult(false, "Target not found");
        }

        return new VerbResult(true, "");
    }
}