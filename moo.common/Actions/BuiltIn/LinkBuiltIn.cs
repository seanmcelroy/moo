using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class LinkBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if (string.Compare(verb, "@link", StringComparison.OrdinalIgnoreCase) == 0 && command.HasDirectObject())
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, ILogger? logger, CancellationToken cancellationToken)
        {
            var str = command.GetNonVerbPhrase();
            if (str == null || str.Length < 3)
                return new VerbResult(false, "@link object1=object2 [; object3; ... objectn ]..\r\nLinks object1 to object2, provided you control object1, and object2 is either controlled by you or linkable. Actions may be linked to more than one thing, specified in a list separated by semi-colons.");

            var parts = str.Split("=");
            if (parts.Length != 2)
                return new VerbResult(false, "@link object1=object2 [; object3; ... objectn ]..\r\nLinks object1 to object2, provided you control object1, and object2 is either controlled by you or linkable. Actions may be linked to more than one thing, specified in a list separated by semi-colons.");

            var thingName = parts[0];
            var destNames = parts[1].TrimEnd('\r', '\n', ' ', '\t');

            var thingDbref = await Matcher.InitObjectSearch(player, thingName, Dbref.DbrefObjectType.Exit, cancellationToken)
                .MatchEverything()
                .NoisyResult();

            if (thingDbref.Equals(Dbref.NOT_FOUND))
                return new VerbResult(false, $"Can't find '{thingName}' here");
            if (thingDbref.Equals(Dbref.AMBIGUOUS))
                return new VerbResult(false, "Which one?");

            switch (thingDbref.Type)
            {
                // We're trying to link an exit to something else.
                case Dbref.DbrefObjectType.Exit:
                    {
                        var exitLookup = await ThingRepository.Instance.GetAsync<Exit>(thingDbref, cancellationToken);
                        if (!exitLookup.isSuccess || exitLookup.value == null)
                            return new VerbResult(false, $"Cannot retrieve {thingDbref}.  {exitLookup.reason}");

                        var exit = exitLookup.value;
                        if (exit.LinkTargets.Count > 0)
                        {
                            var (playerControlsExit, _) = await exit.IsControlledBy(player, cancellationToken);
                            if (playerControlsExit)
                                return new VerbResult(false, "That exit is already linked.");
                            else
                                return new VerbResult(false, "Permission denied. (you don't control the exit to relink)");
                        }

                        // TODO: Handle costs https://github.com/fuzzball-muck/fuzzball/blob/b0ea12f4d40a724a16ef105f599cb8b6a037a77a/src/create.c#L171

                        // Do it.
                        exit.owner = await player.GetOwner(cancellationToken);
                        var newLinks = await Exit.ParseLinks(player, exit, destNames, false, cancellationToken);
                        exit.SetLinkTargets(newLinks);
                        break;
                    }
            }

            return new VerbResult(true, $"Linked.");
        }
    }
}