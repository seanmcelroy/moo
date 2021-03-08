using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class LinkBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(PlayerConnection connection, CommandResult command)
        {
            var verb = command.getVerb().ToLowerInvariant();
            if (string.Compare(verb, "@link", StringComparison.OrdinalIgnoreCase) == 0 && command.hasDirectObject())
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
        {
            var str = command.getNonVerbPhrase();
            if (str == null || str.Length < 3)
                return new VerbResult(false, "@link object1=object2 [; object3; ... objectn ]..\r\nLinks object1 to object2, provided you control object1, and object2 is either controlled by you or linkable. Actions may be linked to more than one thing, specified in a list separated by semi-colons.");

            var parts = str.Split("=");
            if (parts.Length != 2)
                return new VerbResult(false, "@link object1=object2 [; object3; ... objectn ]..\r\nLinks object1 to object2, provided you control object1, and object2 is either controlled by you or linkable. Actions may be linked to more than one thing, specified in a list separated by semi-colons.");

            var thingName = parts[0];
            var destNames = parts[1].TrimEnd('\r', '\n', ' ', '\t');

            var thingDbref = await Matcher.InitObjectSearch(connection.GetPlayer(), thingName, Dbref.DbrefObjectType.Exit, cancellationToken)
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
                            if (await exit.IsControlledBy(connection, cancellationToken))
                                return new VerbResult(false, "That exit is already linked.");
                            else
                                return new VerbResult(false, "Permission denied. (you don't control the exit to relink)");
                        }

                        // TODO: Handle costs https://github.com/fuzzball-muck/fuzzball/blob/b0ea12f4d40a724a16ef105f599cb8b6a037a77a/src/create.c#L171

                        // Do it.
                        exit.owner = connection.GetPlayer().owner;
                        var newLinks = await Exit.ParseLinks(connection.GetPlayer(), exit, destNames, false, cancellationToken);
                        exit.SetLinkTargets(newLinks);
                        break;
                    }
            }

            return new VerbResult(true, $"Linked.");
        }
    }
}