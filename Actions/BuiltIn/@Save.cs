using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using static ThingRepository;

public class @Save : Action
{
    public sealed override bool CanProcess(Player player, CommandResult command)
    {
        string verb = command.getVerb().ToLowerInvariant();
        return (verb == "@save" && command.hasDirectObject());
    }

    public override ActionType Type => ActionType.BuiltIn;

    public sealed override async Task<VerbResult> Process(Player player, CommandResult command, CancellationToken cancellationToken)
    {
        int? targetId = command.resolveDirectObject(player);
        if (targetId == null)
        {
            await player.sendOutput("I don't see that here.");
            return new VerbResult(false, "Target not found");
        }

        GetResult<Thing> lookup = await ThingRepository.GetAsync<Thing>(targetId.Value, cancellationToken);
        if (lookup.isSuccess)
        {
            var target = lookup.value;
            var serialized = target.Serialize();
            var rebuilt = (Thing)typeof(Thing).GetMethod("Deserialize").MakeGenericMethod(target.GetType()).Invoke(null, new object[] { serialized });
            var reserialized = rebuilt.Serialize();

            if (string.Compare(serialized, reserialized) != 0)
            {
                await player.sendOutput(">>> [CRITICAL] Serialization verification failed.  Object will be corrupted.");
                await player.sendOutput("First serialization pass:");
                await player.sendOutput(serialized);
                await player.sendOutput("Second serialization pass:");
                await player.sendOutput(reserialized);
            }
            else
            {
                await player.sendOutput("Serialization check passed.");
                var success = await ThingRepository.FlushToDatabaseAsync(target, cancellationToken);
                await player.sendOutput($"Save to database: {success}");
            }
        }
        else
        {
            await player.sendOutput("You can't seem to find that.");
            return new VerbResult(false, "Target not found");
        }

        return new VerbResult(true, "");
    }
}