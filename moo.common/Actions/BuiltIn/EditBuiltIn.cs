using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static ThingRepository;

public class EditBuiltIn : IRunnable
{
    public Tuple<bool, string> CanProcess(PlayerConnection connection, CommandResult command)
    {
        var verb = command.getVerb().ToLowerInvariant();
        if (verb == "@edit" && command.hasDirectObject())
            return new Tuple<bool, string>(true, verb);
        return new Tuple<bool, string>(false, null);
    }

    public async Task<VerbResult> Process(PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        var str = command.getNonVerbPhrase();
        if (str == null || str.Trim().Length == 0)
            return new VerbResult(false, "@edit <program>\r\nSearches for a program and if a match is found, puts the player into edit mode. Programs must be created with @PROGRAM.");

        var sourceDbref = await connection.FindThingForThisPlayerAsync(str, cancellationToken);
        if (sourceDbref.Equals(Dbref.NOT_FOUND))
            return new VerbResult(false, $"Can't find '{str}' here");
        if (sourceDbref.Equals(Dbref.AMBIGUOUS))
            return new VerbResult(false, $"Which one?");

        var sourceLookup = await ThingRepository.GetAsync<Script>(sourceDbref, cancellationToken);
        if (!sourceLookup.isSuccess)
        {
            await connection.sendOutput($"You can't seem to find that.  {sourceLookup.reason}");
            return new VerbResult(false, "Target not found");
        }

        var program = sourceLookup.value;

        if (program.Type != Dbref.DbrefObjectType.Program)
            return new VerbResult(false, $"Only programs can be edited, but {program.UnparseObject()} is not one.");

        if (!await program.IsControlledByAsync(connection, cancellationToken))
            return new VerbResult(false, $"You don't control {program.id.ToString()}");

        connection.EnterEditMode(command.getDirectObject(), async t =>
        {
            program.programText += $"\n{t}";
            program.Uncompile();
            await program.Compile();
        });

        return new VerbResult(true, "Editor initiated");
    }
}