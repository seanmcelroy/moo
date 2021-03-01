using System;
using System.Threading;
using System.Threading.Tasks;

public class ChownBuiltIn : IRunnable
{
    public Tuple<bool, string?> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (string.Compare(verb, "@chown", StringComparison.OrdinalIgnoreCase) == 0 && command.hasDirectObject())
            return new Tuple<bool, string?>(true, verb);
        return new Tuple<bool, string?>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        var str = command.getNonVerbPhrase();
        if (str == null || string.IsNullOrWhiteSpace(str))
            return new VerbResult(false, "@CHOWN <object> [=<player>]\r\n\r\nChanges the ownership of <object> to <player>, or if no player is given, to yourself. Any player is allowed to take possession of objects, rooms, and actions, provided the CHOWN_OK flag is set. Mortals cannot take ownership of a room unless they are standing in it, and may not take ownership of an object unless they are holding it. Wizards have absolute power over all ownership.");

        var targetString = str.Split('=')[0].Trim();
        var ownerDbref = connection.Dbref;
        if (str.Split('=').Length > 1)
        {
            var ownerString = str.Split('=')[1].Trim();
            if (ownerDbref.Equals(Dbref.NOT_FOUND))
                return new VerbResult(false, "I couldn't find that player.");
            if (ownerDbref.Equals(Dbref.AMBIGUOUS))
                return new VerbResult(false, "Which one?");
            if (ownerDbref != connection.Dbref && !connection.GetPlayer().HasFlag(Thing.Flag.WIZARD))
                return new VerbResult(false, "Only wizards can transfer ownership to others.");
            var ownerThing = await ThingRepository.GetAsync<Thing>(ownerDbref, cancellationToken);
            if (ownerThing.value == null || ownerThing.value.type != (int)Dbref.DbrefObjectType.Player)
                return new VerbResult(false, "I couldn't find that player."); // Must be a player
        }

        var targetDbref = await connection.FindThingForThisPlayerAsync(targetString, cancellationToken);
        if (targetDbref.Equals(Dbref.NOT_FOUND))
            return new VerbResult(false, $"Can't find '{targetString}' here");
        if (targetDbref.Equals(Dbref.AMBIGUOUS))
            return new VerbResult(false, "Which one?");

        var targetLookup = await ThingRepository.GetAsync<Thing>(targetDbref, cancellationToken);
        if (!targetLookup.isSuccess || targetLookup.value == null)
        {
            await connection.sendOutput($"You can't seem to find that.  {targetLookup.reason}");
            return new VerbResult(false, "Target not found");
        }

        var target = targetLookup.value;
        target.owner = ownerDbref;
        await ThingRepository.FlushToDatabaseAsync(target, cancellationToken);
        return new VerbResult(true, $"Owner of {target.UnparseObject()} changed to {ownerDbref}.");
    }
}