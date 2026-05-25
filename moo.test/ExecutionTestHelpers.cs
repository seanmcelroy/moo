using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using moo.common;
using moo.common.Models;
using moo.common.Scripting;

namespace moo.Test
{
    /// <summary>
    /// Compiles + runs a MUF program end-to-end through the full preprocessor,
    /// tokenizer, and ForthProcess pipeline, then returns the resulting stack
    /// so tests can assert on observable execution behavior.
    /// </summary>
    public static class ExecutionTestHelpers
    {
        private static int initialized;

        private static void EnsureServer()
        {
            if (Interlocked.CompareExchange(ref initialized, 1, 0) == 0)
            {
                try { Server.Initialize(null); }
                catch (InvalidOperationException) { /* already initialized in a prior fixture */ }
            }
        }

        public static async Task<(ForthWordResult result, Stack<ForthDatum> stack)> RunProgramAsync(
            string programText,
            params object[] args)
        {
            EnsureServer();

            var pre = await ForthPreprocessor.Preprocess(Dbref.NOT_FOUND, null, programText, CancellationToken.None);
            if (!pre.IsSuccessful)
                throw new InvalidOperationException("Preprocess failed: " + pre.Reason);

            var locals = pre.ProgramLocalVariables ?? new Dictionary<string, ForthVariable>();
            var tok = await ForthTokenizer.Tokenzie(null, pre.ProcessedProgram!, locals, null);
            if (!tok.IsSuccessful)
                throw new InvalidOperationException("Tokenize failed: " + tok.Reason);

            var process = new ForthProcess(Dbref.NOT_FOUND, Dbref.NOT_FOUND, Dbref.NOT_FOUND, "test", 0);
            if (tok.ProgramLocalVariables != null)
                foreach (var v in tok.ProgramLocalVariables)
                    process.SetProgramLocalVariable(v.Key, v.Value);

            var result = await process.RunAsync(tok.Words, Dbref.NOT_FOUND, "test", args ?? [],
                null, CancellationToken.None);

            return (result, process.Stack);
        }
    }
}
