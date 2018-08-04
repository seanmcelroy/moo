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
        var forth = new ForthInterpreter(server, programText);
        var result = await forth.SpawnAsync(id, connection, connection.Dbref, command.getVerb(), new[] { command.getNonVerbPhrase() }, cancellationToken);
        var scriptResult = new VerbResult(result.IsSuccessful, result.Reason?.ToString());
        return scriptResult;
    }
}