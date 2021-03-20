using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using moo.common.Connections;
using moo.common.Scripting;

namespace moo.common.Models
{
    public class Script : Thing, IRunnable
    {
        public string? programText;

        private ForthTokenizerResult tokenized;

        public Script() => type = (int)Dbref.DbrefObjectType.Program;

        public virtual Tuple<bool, string?> CanProcess(Dbref player, CommandResult command)
        {
            string verb = command.GetVerb().ToLowerInvariant();

            foreach (var key in new[] { this.id.ToString(), name })
            {
                if (string.Compare(key, verb, true) == 0)
                    return new Tuple<bool, string?>(true, verb);
            }

            return new Tuple<bool, string?>(false, null);
        }

        public static async Task<Tuple<bool, string, ForthTokenizerResult>> CompileAsync(Script script, string programText, PlayerConnection connection, CancellationToken cancellationToken)
        {
            // Set 1: Preprocessing directives
            var preprocessed = await ForthPreprocessor.Preprocess(connection, script, programText, cancellationToken);
            if (!preprocessed.IsSuccessful)
                return new Tuple<bool, string, ForthTokenizerResult>(false, preprocessed.Reason, default);

            // Step 2: Tokenization
            var tokenized = await ForthTokenizer.Tokenzie(null, preprocessed.ProcessedProgram, preprocessed.ProgramLocalVariables);
            if (!tokenized.IsSuccessful)
                return new Tuple<bool, string, ForthTokenizerResult>(false, tokenized.Reason, default);

            return new Tuple<bool, string, ForthTokenizerResult>(true, "Compiled", tokenized);
        }

        public async Task<Tuple<bool, string>> CompileAsync(PlayerConnection connection, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(programText))
                return new Tuple<bool, string>(true, "No program text provided");

            if (default(ForthTokenizerResult).Equals(tokenized))
            {
                var result = await CompileAsync(this, programText, connection, cancellationToken);
                if (!result.Item1)
                    return new Tuple<bool, string>(false, result.Item2);

                tokenized = result.Item3;
                return new Tuple<bool, string>(true, result.Item2);
            }

            return new Tuple<bool, string>(true, "Program already compiled");
        }

        public void Uncompile() => tokenized = default;

        protected override Dictionary<string, object?> GetSerializedElements()
        {
            var results = base.GetSerializedElements();
            results.Add("programText", programText);
            return results;
        }

        public async Task<VerbResult> Process(
            Dbref player,
             PlayerConnection? connection,
            CommandResult command,
            CancellationToken cancellationToken)
        {
            // TODO: Right now we block on programs

            byte effectiveMuckerLevel =
                HasFlag(Flag.WIZARD) ? 4
                : HasFlag(Flag.LEVEL_3) ? 3
                : HasFlag(Flag.LEVEL_2) ? 2
                : HasFlag(Flag.LEVEL_1) ? 1
                : 0;

            var process = new ForthProcess(id, effectiveMuckerLevel, connection)
            {
                State = ForthProcess.ProcessState.Parsing
            };

            var compileResult = await CompileAsync(connection, cancellationToken);

            if (!compileResult.Item1)
            {
                process.State = ForthProcess.ProcessState.Complete;
                return new VerbResult(false, compileResult.Item2);
            }

            if (tokenized.ProgramLocalVariables != null)
                foreach (var v in tokenized.ProgramLocalVariables)
                    process.SetProgramLocalVariable(v.Key, v.Value);

            var result = await Server.ExecuteAsync(process, tokenized.Words, connection.Dbref,
                command.GetVerb(),
                new[] { command.GetNonVerbPhrase() },
                cancellationToken);
            var scriptResult = new VerbResult(result.IsSuccessful, result.Reason?.ToString());

            process.State = ForthProcess.ProcessState.Complete;
            return scriptResult;
        }
    }
}