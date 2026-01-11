using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace moo.common.Scripting
{
    public readonly struct ForthTokenizerResult
    {
        private readonly bool isSuccessful;
        private readonly string reason;
        private readonly List<ForthWord>? words;
        private readonly Dictionary<string, ForthVariable>? programLocalVariables;

        public readonly bool IsSuccessful => isSuccessful;
        public readonly string Reason => reason;
        public readonly ReadOnlyCollection<ForthWord> Words => words?.AsReadOnly() ?? ReadOnlyCollection<ForthWord>.Empty;
        public readonly Dictionary<string, ForthVariable> ProgramLocalVariables => programLocalVariables ?? [];

        public ForthTokenizerResult(string failureReason)
        {
            isSuccessful = false;
            reason = failureReason;
            words = null;
            programLocalVariables = null;
        }

        public ForthTokenizerResult(List<ForthWord> words, Dictionary<string, ForthVariable> programLocalVariables)
        {
            isSuccessful = true;
            reason = string.Empty;
            this.words = words;
            this.programLocalVariables = programLocalVariables;
        }
    }
}