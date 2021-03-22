using System.Collections.Generic;

namespace moo.common.Scripting
{
    public struct ForthPreprocessingResult
    {
        private readonly bool isSuccessful;
        private readonly string? reason;
        private readonly string? processedProgram;
        private readonly List<string>? publicFunctionNames;
        private readonly Dictionary<string, ForthVariable>? programLocalVariables;

        public bool IsSuccessful => isSuccessful;
        public string? Reason => reason;
        public string? ProcessedProgram => processedProgram;
        public Dictionary<string, ForthVariable>? ProgramLocalVariables => programLocalVariables;

        public ForthPreprocessingResult(string failureReason)
        {
            isSuccessful = false;
            reason = failureReason;
            processedProgram = null;
            publicFunctionNames = null;
            programLocalVariables = null;
        }

        public ForthPreprocessingResult(string processedProgram, List<string> publicFunctionNames, Dictionary<string, ForthVariable>? programLocalVariables)
        {
            isSuccessful = true;
            reason = null;
            this.processedProgram = processedProgram;
            this.publicFunctionNames = publicFunctionNames;
            this.programLocalVariables = programLocalVariables;
        }
    }
}