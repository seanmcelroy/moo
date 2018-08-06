using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using static ForthVariable;

public class ForthInterpreter
{
    private readonly Server server;
    private readonly string program;
    private ForthParseResult parsed;

    public ForthInterpreter(Server server, string program)
    {
        this.server = server;
        this.program = program;
    }
    public async Task<ForthProgramResult> SpawnAsync(
        Dbref scriptId,
        PlayerConnection connection,
        Dbref trigger,
        string command,
        object[] args,
        CancellationToken cancellationToken)
    {
        if (default(ForthParseResult).Equals(parsed))
            parsed = ForthParser.ParseProgram(connection, program);
        if (!parsed.IsSuccessful) {
            parsed = default(ForthParseResult);
            return new ForthProgramResult(ForthProgramResult.ForthProgramErrorResult.SYNTAX_ERROR, parsed.Reason);
        }

        var process = new ForthProcess(server, scriptId, parsed.Words, connection);
        foreach (var v in parsed.ProgramLocalVariables)
            process.SetProgramLocalVariable(v.Key, v.Value);

        return await server.ExecuteAsync(process, trigger, command, args, cancellationToken);
    }
}