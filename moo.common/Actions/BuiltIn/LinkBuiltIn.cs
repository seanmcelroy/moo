using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class LinkBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@link" && command.hasDirectObject())
            return new Tuple<bool, string>(true, verb);
        return new Tuple<bool, string>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        var str = command.getNonVerbPhrase();
        if (str == null || str.Length < 3)
            return new VerbResult(false, "@link object1=object2 [; object3; ... objectn ]..\r\nLinks object1 to object2, provided you control object1, and object2 is either controlled by you or linkable. Actions may be linked to more than one thing, specified in a list separated by semi-colons.");

        var parts = str.Split("=");
        if (parts.Length != 2)
            return new VerbResult(false, "@link object1=object2 [; object3; ... objectn ]..\r\nLinks object1 to object2, provided you control object1, and object2 is either controlled by you or linkable. Actions may be linked to more than one thing, specified in a list separated by semi-colons.");

        var object1name = parts[0];
        var object2phrase = parts[1];

        var player = connection.GetPlayer();
        var sourceDbref = await player.FindThingForThisPlayerAsync(object1name, cancellationToken);
        if (sourceDbref.Equals(Dbref.NOT_FOUND))
            return new VerbResult(false, $"Can't find '{object1name}' here");
        if (sourceDbref.Equals(Dbref.AMBIGUOUS))
            return new VerbResult(false, $"Which one?");

        var sourceLookup = await ThingRepository.GetAsync<Thing>(sourceDbref, cancellationToken);
        if (!sourceLookup.isSuccess)
        {
            await connection.sendOutput($"You can't seem to find that.  {sourceLookup.reason}");
            return new VerbResult(false, "object1 not found");
        }

        var source = sourceLookup.value;
        if (!await source.IsControlledByAsync(player, cancellationToken))
            return new VerbResult(false, $"You don't control {source.id.ToString()}");

        var linkTargets = new List<Dbref>();
        foreach (var targetName in object2phrase.Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            var targetDbref = await player.FindThingForThisPlayerAsync(targetName, cancellationToken);
            if (targetDbref.Equals(Dbref.NOT_FOUND))
                return new VerbResult(false, $"Can't find '{targetName}' here");
            if (targetDbref.Equals(Dbref.AMBIGUOUS))
                return new VerbResult(false, $"Which one?");

            var targetLookup = await ThingRepository.GetAsync<Thing>(targetDbref, cancellationToken);
            if (!targetLookup.isSuccess)
            {
                await connection.sendOutput($"You can't seem to find that.  {targetLookup.reason}");
                return new VerbResult(false, "object2 not found");
            }

            var target = targetLookup.value;
            var controlsTarget = await target.IsControlledByAsync(player, cancellationToken);

            if (target.Type == Dbref.DbrefObjectType.Room &&
                !source.HasFlag(Thing.Flag.LINK_OK) &&
                !source.HasFlag(Thing.Flag.ABODE) &&
                !controlsTarget)
                return new VerbResult(false, $"You don't control {target.id.ToString()}");
            else if (!controlsTarget)
                return new VerbResult(false, $"You don't control {target.id.ToString()}");

            linkTargets.Add(targetLookup.value.id);
        }

        source.SetLinkTargets(linkTargets.ToArray());

        return new VerbResult(true, $"Linked.");
    }
}