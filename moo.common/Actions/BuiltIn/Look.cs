using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class Look : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if ("look".StartsWith(verb, StringComparison.OrdinalIgnoreCase))
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, CancellationToken cancellationToken)
        {
            var str = command.GetNonVerbPhrase();
            if (str == null || string.IsNullOrWhiteSpace(str) || string.Compare("here", str, StringComparison.OrdinalIgnoreCase) == 0)
            {
                // Look at a room
                if (connection != null)
                    return await LookRoom(player, connection, cancellationToken);
                return new VerbResult(false, $"No connection for look result");
            }

            // Look at a thing
            var ownerIsWizard = await (await player.GetOwner(cancellationToken)).IsWizard(cancellationToken);
            var targetDbref = await Matcher.InitObjectSearch(player, str, Dbref.DbrefObjectType.Unknown, cancellationToken)
                .MatchAllExits()
                .MatchNeighbor()
                .MatchPossession()
                .MatchAbsolute(ownerIsWizard)
                .MatchPlayer(ownerIsWizard)
                .MatchHere()
                .MatchMe()
                .Result();

            return new VerbResult(true, $"Object {targetDbref} looked at");
        }

        private static async Task<VerbResult> LookRoom(Dbref player, PlayerConnection connection, CancellationToken cancellationToken)
        {
            var locationDbref = await player.GetLocation(cancellationToken);
            if (locationDbref == Dbref.NOT_FOUND)
                return new VerbResult(false, $"Room {locationDbref} not found");

            // Room name
            var (unparsed, loc) = await locationDbref.UnparseObject(player, cancellationToken);
            await connection.SendOutput(unparsed);

            // Room description
            if (loc?.Type == Dbref.DbrefObjectType.Room)
            {
                if (!string.IsNullOrWhiteSpace(loc.externalDescription))
                    await connection.SendOutput(loc.externalDescription);
            }

            // Room contents
            if (loc?.Contents != null)
                foreach (var content in loc.Contents)
                {
                    var contentLookup = await ThingRepository.Instance.GetAsync<Thing>(content, cancellationToken);
                    if (!contentLookup.isSuccess || contentLookup.value == null)
                        continue;

                    // TODO look_contents logic
                    await connection.SendOutput(await contentLookup.value.UnparseObject(player, cancellationToken));
                }

            return new VerbResult(true, $"Room {unparsed} looked at");
        }
    }
}