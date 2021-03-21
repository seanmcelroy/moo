using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class LoadBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if (string.Compare(verb, "@load", StringComparison.OrdinalIgnoreCase) == 0 && command.HasDirectObject())
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, CancellationToken cancellationToken)
        {
            var targetDbref = await Matcher
               .InitObjectSearch(player, command.GetDirectObject(), Dbref.DbrefObjectType.Unknown, cancellationToken)
               .MatchEverything()
               .NoisyResult();

            if (targetDbref == Dbref.NOT_FOUND)
                return new VerbResult(false, "Target not found");

            var lookup = await ThingRepository.Instance.GetAsync<Thing>(targetDbref, cancellationToken);
            if (lookup.isSuccess && lookup.value != null)
            {
                var loadResult = await ThingRepository.Instance.LoadFromDatabaseAsync<Thing>(targetDbref, cancellationToken);
                await Server.NotifyAsync(player, $"Load from database: {loadResult.isSuccess}");
            }
            else
            {
                await Server.NotifyAsync(player, "You can't seem to find that.");
                return new VerbResult(false, "Target not found");
            }

            return new VerbResult(true, "");
        }
    }
}