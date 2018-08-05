using System.Collections.Generic;
using System.Collections.ObjectModel;

public struct ForthParseResult
{
    private readonly bool isSuccessful;
    private readonly string reason;
    private readonly List<ForthWord> words;
    private readonly Dictionary<string, ForthVariable> programLocalVariables;

    public bool IsSuccessful => isSuccessful;
    public string Reason => reason;
    public ReadOnlyCollection<ForthWord> Words => words.AsReadOnly();
    public Dictionary<string, ForthVariable> ProgramLocalVariables => programLocalVariables;

    public ForthParseResult(string failureReason)
    {
        this.isSuccessful = false;
        this.reason = failureReason;
        this.words = null;
        this.programLocalVariables = null;
    }

    public ForthParseResult(List<ForthWord> words, Dictionary<string, ForthVariable> programLocalVariables)
    {
        this.isSuccessful = true;
        this.reason = null;
        this.words = words;
        this.programLocalVariables = programLocalVariables;
    }
}