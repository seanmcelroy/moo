using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Script : Action
{
    public string programText;

    public sealed override async Task<VerbResult> Process(Server server, PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        // TODO: Right now we block on programs
        var process = new ForthProcess(server, id, connection);

        process.State = ForthProcess.ProcessState.Parsing;
        var parsed = ForthParser.ParseProgram(connection, programText);
        if (!parsed.IsSuccessful)
        {
            process.State = ForthProcess.ProcessState.Complete;
            return new VerbResult(false, parsed.Reason);
        }

        foreach (var v in parsed.ProgramLocalVariables)
            process.SetProgramLocalVariable(v.Key, v.Value);

        var result = await server.ExecuteAsync(process, parsed.Words, connection.Dbref, command.getVerb(), new[] { command.getNonVerbPhrase() }, cancellationToken);
        var scriptResult = new VerbResult(result.IsSuccessful, result.Reason?.ToString());

        process.State = ForthProcess.ProcessState.Complete;
        return scriptResult;
    }
}