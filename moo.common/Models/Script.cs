using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Script : Action
{
    public string programText;

    private ForthTokenizerResult tokenized;

    public sealed override async Task<VerbResult> Process(Server server, PlayerConnection connection, CommandResult command, CancellationToken cancellationToken)
    {
        // TODO: Right now we block on programs
        var process = new ForthProcess(server, id, connection);

        process.State = ForthProcess.ProcessState.Parsing;

        if (default(ForthTokenizerResult).Equals(tokenized))
        {
            // Set 1: Preprocessing directives
            var preprocessed = await ForthPreprocessor.Preprocess(connection, programText);
            if (!preprocessed.IsSuccessful)
            {
                process.State = ForthProcess.ProcessState.Complete;
                return new VerbResult(false, preprocessed.Reason);
            }

            // Step 2: Tokenization
            tokenized = await ForthTokenizer.Tokenzie(connection, preprocessed.ProcessedProgram, preprocessed.ProgramLocalVariables);
            if (!tokenized.IsSuccessful)
            {
                process.State = ForthProcess.ProcessState.Complete;
                return new VerbResult(false, tokenized.Reason);
            }
        }

        foreach (var v in tokenized.ProgramLocalVariables)
            process.SetProgramLocalVariable(v.Key, v.Value);

        var result = await server.ExecuteAsync(process, tokenized.Words, connection.Dbref, command.getVerb(), new[] { command.getNonVerbPhrase() }, cancellationToken);
        var scriptResult = new VerbResult(result.IsSuccessful, result.Reason?.ToString());

        process.State = ForthProcess.ProcessState.Complete;
        return scriptResult;
    }
}