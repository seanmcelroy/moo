using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using moo.common.Connections;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class ProgramBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            string verb = command.GetVerb().ToLowerInvariant();

            foreach (var key in new[] { "@prog", "@program" })
            {
                if (string.Compare(key, verb, true) == 0)
                    return new Tuple<bool, string?>(true, verb);
            }

            return new Tuple<bool, string?>(false, null);
        }

        public Task<VerbResult> Process(Dbref player, PlayerConnection? connection, CommandResult command, ILogger? logger, CancellationToken cancellationToken)
        {
            var script = Server.RegisterScript(command.GetDirectObject(), connection.GetPlayer());
            connection.EnterEditMode(script, command.GetDirectObject(), async t =>
            {
                // Move this to my inventory
                script.programText = t;
                await script.MoveToAsync(connection, cancellationToken);     

                logger?.LogInformation("Created new program {name}", script.UnparseObjectInternal());
            });

            return Task.FromResult(new VerbResult(true, "Editor initiated"));
        }
    }
}