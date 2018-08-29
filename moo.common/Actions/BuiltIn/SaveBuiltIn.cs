using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using static ThingRepository;

public class SaveBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@save" && command.hasDirectObject())
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
            var serialized = target.Serialize();
            var rebuilt = (Thing)typeof(Thing).GetMethod("Deserialize").MakeGenericMethod(target.GetType()).Invoke(null, new object[] { serialized });
            var reserialized = rebuilt.Serialize();

            if (string.Compare(serialized, reserialized) != 0)
            {
                await connection.sendOutput(">>> [CRITICAL] Serialization verification failed.  Object will be corrupted.");
                await connection.sendOutput("First serialization pass:");
                await connection.sendOutput(serialized);
                await connection.sendOutput("Second serialization pass:");
                await connection.sendOutput(reserialized);
            }
            else
            {
                await connection.sendOutput("Serialization check passed.");
                var success = await ThingRepository.FlushToDatabaseAsync(target, cancellationToken);
                await connection.sendOutput($"Save to database: {success}");
            }
        }
        else
        {
            await connection.sendOutput("You can't seem to find that.");
            return new VerbResult(false, "Target not found");
        }

        return new VerbResult(true, "");
    }
}