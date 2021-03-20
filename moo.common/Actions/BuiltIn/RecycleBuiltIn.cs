using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class RecycleBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if (string.Compare(verb, "@recycle", StringComparison.OrdinalIgnoreCase) == 0 && command.HasDirectObject())
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

            var targetLookup = await ThingRepository.Instance.GetAsync<Thing>(targetDbref, cancellationToken);
            if (targetLookup.isSuccess && targetLookup.value != null)
            {
                var target = targetLookup.value;
                var serialized = target.Serialize();
                var rebuilt = (Thing)typeof(Thing).GetMethod("Deserialize")!.MakeGenericMethod(target.GetType()).Invoke(null, new object[] { serialized })!;
                var reserialized = rebuilt!.Serialize();

                if (string.Compare(serialized, reserialized) != 0)
                {
                    await connection.SendOutput(">>> [CRITICAL] Serialization verification failed.  Object will be corrupted.");
                    await connection.SendOutput("First serialization pass:");
                    await connection.SendOutput(serialized);
                    await connection.SendOutput("Second serialization pass:");
                    await connection.SendOutput(reserialized);
                }
                else
                {
                    await connection.SendOutput("Serialization check passed.");
                    var success = await ThingRepository.Instance.FlushToDatabaseAsync(target, cancellationToken);
                    await connection.SendOutput($"Save to database: {success}");
                }
            }
            else
            {
                await connection.SendOutput("You can't seem to find that.");
                return new VerbResult(false, "Target not found");
            }

            return new VerbResult(true, "");
        }
    }
}