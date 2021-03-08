using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class SaveBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(PlayerConnection connection, CommandResult command)
        {
            var verb = command.getVerb().ToLowerInvariant();
            if (string.Compare(verb, "@save", StringComparison.OrdinalIgnoreCase) == 0 && command.hasDirectObject())
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
                var target = lookup.value;
                var serialized = target.Serialize();
                var rebuilt = (Thing)typeof(Thing).GetMethod("Deserialize")!.MakeGenericMethod(target.GetType()).Invoke(null, new object[] { serialized })!;
                var reserialized = rebuilt!.Serialize();

                if (string.Compare(serialized, reserialized) != 0)
                {
                    await connection.sendOutput(">>> [CRITICAL] Serialization verification failed.  Object will be corrupted.");
                    await connection.sendOutput("First serialization pass:");
                    await connection.sendOutput(serialized);
                    await connection.sendOutput("Second serialization pass:");
                    await connection.sendOutput(reserialized);
                }
                else
                {
                    await connection.sendOutput("Serialization check passed.");
                    var success = await ThingRepository.Instance.FlushToDatabaseAsync(target, cancellationToken);
                    await connection.sendOutput($"Save to database: {success}");
                }
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