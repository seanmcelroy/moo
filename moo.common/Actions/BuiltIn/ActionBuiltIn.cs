using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class ActionBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if (string.Compare(verb, "@action", StringComparison.OrdinalIgnoreCase) == 0 && command.HasDirectObject())
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, ILogger? logger, CancellationToken cancellationToken)
        {
            var str = command.GetNonVerbPhrase();
            if (str == null || str.Length < 3)
                return new VerbResult(false, "@action name=source[=regname].\r\nCreates a new action and attaches it to the thing, room, or player specified. If a regname is specified, then the _reg/regname property on the player is set to the dbref of the new object. This lets players refer to the object as $regname (ie: $mybutton) in @locks, @sets, etc. You may only attach actions you control to things you control. Creating an action costs 1 penny. The action can then be linked with the command @LINK.");

            var parts = str.Split("=");
            if (parts.Length < 2 || parts.Length > 3)
                return new VerbResult(false, "@action name=source[=regname].\r\nCreates a new action and attaches it to the thing, room, or player specified. If a regname is specified, then the _reg/regname property on the player is set to the dbref of the new object. This lets players refer to the object as $regname (ie: $mybutton) in @locks, @sets, etc. You may only attach actions you control to things you control. Creating an action costs 1 penny. The action can then be linked with the command @LINK.");

            var name = parts[0].Trim();
            var sourcePhrase = parts[1].Trim();
            var regname = parts.Length == 3 ? parts[2].Trim() : null;

            var sourceDbref = await Matcher.InitObjectSearch(player, sourcePhrase, Dbref.DbrefObjectType.Unknown, cancellationToken)
                .MatchNeighbor()
                .MatchMe()
                .MatchHere()
                .MatchPossession()
                .MatchRegistered()
                .MatchAbsolute()
                .NoisyResult();

            if (sourceDbref.Equals(Dbref.NOT_FOUND))
                return new VerbResult(false, $"Can't find '{sourcePhrase}' here");
            if (sourceDbref.Equals(Dbref.AMBIGUOUS))
                return new VerbResult(false, "Which one?");

            var source = await sourceDbref.Get(cancellationToken);
            if (source == null)
            {
                await Server.NotifyAsync(player, $"You can't seem to find that.");
                return new VerbResult(false, "Target not found");
            }
            if (source.Type == Dbref.DbrefObjectType.Exit)
                return new VerbResult(false, $"An exit cannot be attached to another exit ({source.id})");
            if (source.Type == Dbref.DbrefObjectType.Program)
                return new VerbResult(false, $"An exit cannot be attached to a program ({source.id})");

            var (result, _) = await source.IsControlledBy(player, cancellationToken);
            if (!result)
                return new VerbResult(false, $"You don't control {source.id}");

            var exit = Exit.Make(name, player, logger);
            var moveResult = await exit.MoveToAsync(source, cancellationToken);
            if (!moveResult.isSuccess)
                await Server.NotifyAsync(player, $"You can't seem to do that on {sourcePhrase}.  {moveResult.reason}");

            if (regname != null)
            {
                var playerObj = await player.Get(cancellationToken);
                if (playerObj == null)
                {
                    await Server.NotifyAsync(player, $"You cannot seem to find the player {player}...");
                    return new VerbResult(false, $"You cannot seem to find the player {player}...");
                }

                playerObj.SetPropertyPathValue($"_reg/{regname}", exit.id);
            }

            return new VerbResult(true, $"Exit {exit.id} created");
        }
    }
}