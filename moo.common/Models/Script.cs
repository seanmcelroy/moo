using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class Script : Thing, IRunnable
{
    public string programText;

    private ForthTokenizerResult tokenized;

    public Script()
    {
        this.type = (int)Dbref.DbrefObjectType.Program;
    }

    public virtual Tuple<bool, string> CanProcess(PlayerConnection player, CommandResult command)
    {
        string verb = command.getVerb().ToLowerInvariant();

        foreach (var key in new[] { this.id.ToString(), name })
        {
            if (string.Compare(key, verb, true) == 0)
                return new Tuple<bool, string>(true, verb);
        }

        return new Tuple<bool, string>(false, null);
    }

    public static async Task<Tuple<bool, string, ForthTokenizerResult>> Compile(string programText)
    {
        // Set 1: Preprocessing directives
        var preprocessed = await ForthPreprocessor.Preprocess(null, programText);
        if (!preprocessed.IsSuccessful)
            return new Tuple<bool, string, ForthTokenizerResult>(false, preprocessed.Reason, default(ForthTokenizerResult));

        // Step 2: Tokenization
        var tokenized = await ForthTokenizer.Tokenzie(null, preprocessed.ProcessedProgram, preprocessed.ProgramLocalVariables);
        if (!tokenized.IsSuccessful)
            return new Tuple<bool, string, ForthTokenizerResult>(false, tokenized.Reason, default(ForthTokenizerResult));

        return new Tuple<bool, string, ForthTokenizerResult>(true, "Compiled", tokenized);
    }

    public async Task<Tuple<bool, string>> Compile()
    {
        if (default(ForthTokenizerResult).Equals(tokenized))
        {
            var result = await Compile(programText);
            if (!result.Item1)
                return new Tuple<bool, string>(false, result.Item2);

            tokenized = result.Item3;
            return new Tuple<bool, string>(true, result.Item2);
        }

        return new Tuple<bool, string>(true, "Program already compiled");
    }

    protected override Dictionary<string, object> GetSerializedElements()
    {
        var results = base.GetSerializedElements();
        results.Add("programText", programText);
        return results;
    }

    public async Task<VerbResult> Process(
        PlayerConnection connection,
        CommandResult command,
        CancellationToken cancellationToken)
    {
        // TODO: Right now we block on programs
        var process = new ForthProcess(id, connection);

        process.State = ForthProcess.ProcessState.Parsing;

        var compileResult = await Compile();

        if (!compileResult.Item1)
        {
            process.State = ForthProcess.ProcessState.Complete;
            return new VerbResult(false, compileResult.Item2);
        }

        if (tokenized.ProgramLocalVariables != null)
            foreach (var v in tokenized.ProgramLocalVariables)
                process.SetProgramLocalVariable(v.Key, v.Value);

        var result = await Server.GetInstance().ExecuteAsync(process, tokenized.Words, connection.Dbref, command.getVerb(), new[] { command.getNonVerbPhrase() }, cancellationToken);
        var scriptResult = new VerbResult(result.IsSuccessful, result.Reason?.ToString());

        process.State = ForthProcess.ProcessState.Complete;
        return scriptResult;
    }
}