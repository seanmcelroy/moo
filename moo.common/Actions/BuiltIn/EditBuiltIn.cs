using System;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Database;
using moo.common.Models;

namespace moo.common.Actions.BuiltIn
{
    public class EditBuiltIn : IRunnable
    {
        public Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            var verb = command.GetVerb().ToLowerInvariant();
            if (string.Compare(verb, "@edit", StringComparison.OrdinalIgnoreCase) == 0 && command.HasDirectObject())
                return new Tuple<bool, string?>(true, verb);
            return new Tuple<bool, string?>(false, null);
        }

        public async Task<VerbResult> Process(
            Dbref player,
            PlayerConnection? connection,
            CommandResult command,
            CancellationToken cancellationToken)
        {
            var str = command.GetNonVerbPhrase();
            if (str == null || str.Trim().Length == 0)
                return new VerbResult(false, "@edit <program>\r\nSearches for a program and if a match is found, puts the player into edit mode. Programs must be created with @PROGRAM.");

            var sourceDbref = await Matcher
                        .InitObjectSearch(player, str, Dbref.DbrefObjectType.Program, cancellationToken)
                        .MatchEverything()
                        .Result();

            if (sourceDbref.Equals(Dbref.NOT_FOUND))
                return new VerbResult(false, $"Can't find '{str}' here");
            if (sourceDbref.Equals(Dbref.AMBIGUOUS))
                return new VerbResult(false, "Which one?");

            var sourceLookup = await ThingRepository.Instance.GetAsync<Script>(sourceDbref, cancellationToken);
            if (!sourceLookup.isSuccess)
            {
                await Server.NotifyAsync(player, $"You can't seem to find that.  {sourceLookup.reason}");
                return new VerbResult(false, "Target not found");
            }

            var program = sourceLookup.value;

            if (program == null)
                return new VerbResult(false, $"No such program found.");

            if (program.Type != Dbref.DbrefObjectType.Program)
                return new VerbResult(false, $"Only programs can be edited, but {await program.UnparseObject(player, cancellationToken)} is not one.");

            var (result, who) = await program.IsControlledBy(player, cancellationToken);
            if (!result)
                return new VerbResult(false, $"You don't control {await program.UnparseObject(player, cancellationToken)}");

            connection.EnterEditMode(null, command.GetDirectObject(), async t =>
            {
                program.programText += $"\n{t}";
                program.Uncompile();
                await program.CompileAsync(player, cancellationToken);
            });

            return new VerbResult(true, "Editor initiated");
        }
    }
}