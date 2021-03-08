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
        public Tuple<bool, string?> CanProcess(PlayerConnection connection, CommandResult command)
        {
            var verb = command.getVerb().ToLowerInvariant();
            if (string.Compare(verb, "@load", StringComparison.OrdinalIgnoreCase) == 0 && command.hasDirectObject())
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
        {
            var targetDbref = await Matcher
               .InitObjectSearch(connection.GetPlayer(), command.getDirectObject(), Dbref.DbrefObjectType.Unknown, cancellationToken)
               .MatchEverything()
               .NoisyResult();

            if (targetDbref == Dbref.NOT_FOUND)
                return new VerbResult(false, "Target not found");

            var lookup = await ThingRepository.Instance.GetAsync<Thing>(targetDbref, cancellationToken);
            if (lookup.isSuccess && lookup.value != null)
            {
                var loadResult = await ThingRepository.Instance.LoadFromDatabaseAsync<Thing>(targetDbref, cancellationToken);
                await connection.sendOutput($"Load from database: {loadResult.isSuccess}");
            }
            else
            {
                await connection.sendOutput("You can't seem to find that.");
                return new VerbResult(false, "Target not found");
            }

            return new VerbResult(true, "");
        }
    }
}