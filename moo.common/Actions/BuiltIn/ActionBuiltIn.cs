using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class ActionBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@action" && command.hasDirectObject())
            return new Tuple<bool, string>(true, verb);
        return new Tuple<bool, string>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        var str = command.getNonVerbPhrase();
        if (str == null || str.Length < 3)
            return new VerbResult(false, "@action name=source[=regname].\r\nCreates a new action and attaches it to the thing, room, or player specified. If a regname is specified, then the _reg/regname property on the player is set to the dbref of the new object. This lets players refer to the object as $regname (ie: $mybutton) in @locks, @sets, etc. You may only attach actions you control to things you control. Creating an action costs 1 penny. The action can then be linked with the command @LINK.");

        var parts = str.Split("=");
        if (parts.Length < 2 || parts.Length > 3)
            return new VerbResult(false, "@action name=source[=regname].\r\nCreates a new action and attaches it to the thing, room, or player specified. If a regname is specified, then the _reg/regname property on the player is set to the dbref of the new object. This lets players refer to the object as $regname (ie: $mybutton) in @locks, @sets, etc. You may only attach actions you control to things you control. Creating an action costs 1 penny. The action can then be linked with the command @LINK.");

        var name = parts[0];
        var sourcePhrase = parts[1];
        var regname = parts.Length == 3 ? parts[2] : null;

        var player = connection.GetPlayer();
        var sourceDbref = await player.FindThingForThisPlayerAsync(sourcePhrase, cancellationToken);
        if (sourceDbref.Equals(Dbref.NOT_FOUND))
            return new VerbResult(false, $"Can't find '{sourcePhrase}' here");
        if (sourceDbref.Equals(Dbref.AMBIGUOUS))
            return new VerbResult(false, $"Which one?");

        var sourceLookup = await ThingRepository.GetAsync<Container>(sourceDbref, cancellationToken);
        if (!sourceLookup.isSuccess)
        {
            await connection.sendOutput($"You can't seem to find that.  {sourceLookup.reason}");
            return new VerbResult(false, "Target not found");
        }

        var source = sourceLookup.value;
        if (!await source.IsControlledByAsync(player, cancellationToken))
            return new VerbResult(false, $"You don't control {source.id.ToString()}");

        var exit = Exit.Make(name, player.id);
        var moveResult = await exit.MoveToAsync(source, cancellationToken);
        if (!moveResult.isSuccess)
            await connection.sendOutput($"You can't seem to do that on {sourcePhrase}.  {moveResult.reason}");

        player.SetPropertyPathValue($"_reg/{regname}", exit.id);

        return new VerbResult(true, $"Exit {exit.id.ToString()} created");
    }
}