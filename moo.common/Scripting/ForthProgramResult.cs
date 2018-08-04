using System.Collections.Generic;

public struct ForthProgramResult
{
    public static ForthProgramResult SUCCESS = new ForthProgramResult
    {
        isSuccessful = true
    };

    public enum ForthProgramErrorResult : byte
    {
        INTERRUPTED,
        STACK_UNDERFLOW,
        TYPE_MISMATCH,
        INVALID_VALUE,
        VARIABLE_NOT_FOUND,
        UNKNOWN_TYPE,
        INTERNAL_ERROR,
        VARIABLE_ALREADY_DEFINED,
        DIVISION_BY_ZERO,
        SYNTAX_ERROR,
        VARIABLE_IS_CONSTANT
    }

    private bool isSuccessful;
    private object result;
    private string reason;
    private Dbref? lastListItem;

    public bool IsSuccessful => isSuccessful;
    public object Result => result;
    public string Reason => reason;
    public Dbref? LastListItem => lastListItem;

    public Dictionary<string, ForthVariable> dirtyVariables;

    public ForthProgramResult(string reason, Dbref? lastListItem = null)
    {
        this.isSuccessful = true;
        this.result = null;
        this.reason = reason;
        this.lastListItem = lastListItem;
        this.dirtyVariables = null;
    }

    public ForthProgramResult(ForthProgramErrorResult errorCode, string reason)
    {
        this.isSuccessful = false;
        this.result = errorCode;
        this.reason = reason ?? System.Enum.GetName(typeof(ForthProgramErrorResult), errorCode);
        this.lastListItem = null;
        this.dirtyVariables = null;
    }
}