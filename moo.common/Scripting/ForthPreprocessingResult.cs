using System.Collections.Generic;
using System.Collections.ObjectModel;

public struct ForthPreprocessingResult
{
    private readonly bool isSuccessful;
    private readonly string reason;
    private readonly string processedProgram;
    private readonly List<string> publicFunctionNames;
    private readonly Dictionary<string, ForthVariable> programLocalVariables;

    public bool IsSuccessful => isSuccessful;
    public string Reason => reason;
    public string ProcessedProgram => processedProgram;
    public Dictionary<string, ForthVariable> ProgramLocalVariables => programLocalVariables;

    public ForthPreprocessingResult(string failureReason)
    {
        this.isSuccessful = false;
        this.reason = failureReason;
        this.processedProgram = null;
        this.publicFunctionNames = null;
        this.programLocalVariables = null;
    }

    public ForthPreprocessingResult(string processedProgram, List<string> publicFunctionNames, Dictionary<string, ForthVariable> programLocalVariables)
    {
        this.isSuccessful = true;
        this.reason = null;
        this.processedProgram = processedProgram;
        this.publicFunctionNames = publicFunctionNames;
        this.programLocalVariables = programLocalVariables;
    }
}