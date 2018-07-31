using System.Collections.Generic;

public struct ForthProgramResult
{

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
        SYNTAX_ERROR
    }

    public bool isSuccessful;
    public object result;
    public string reason;

    public Dictionary<string, object> dirtyVariables;

    public ForthProgramResult(object result, string reason = null)
    {
        this.isSuccessful = true;
        this.result = result;
        this.reason = reason;
        this.dirtyVariables = null;
    }

    public ForthProgramResult(ForthProgramErrorResult errorCode, string reason)
    {
        this.isSuccessful = false;
        this.result = errorCode;
        this.reason = reason ?? System.Enum.GetName(typeof(ForthProgramErrorResult), errorCode);
        this.dirtyVariables = null;
    }
}