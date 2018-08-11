using System.Collections.Generic;

public struct ForthPrimativeResult
{
    public static ForthPrimativeResult SUCCESS = new ForthPrimativeResult
    {
        isSuccessful = true
    };

    private bool isSuccessful;
    private object result;
    private string reason;
    private Dbref? lastListItem;

    public bool IsSuccessful => isSuccessful;
    public object Result => result;
    public string Reason => reason;
    public Dbref? LastListItem => lastListItem;

    public Dictionary<string, ForthVariable> dirtyVariables;

    public ForthPrimativeResult(string reason, Dbref? lastListItem = null)
    {
        this.isSuccessful = true;
        this.result = null;
        this.reason = reason;
        this.lastListItem = lastListItem;
        this.dirtyVariables = null;
    }

    public ForthPrimativeResult(ForthErrorResult errorCode, string reason)
    {
        this.isSuccessful = false;
        this.result = errorCode;
        this.reason = reason ?? System.Enum.GetName(typeof(ForthErrorResult), errorCode);
        this.lastListItem = null;
        this.dirtyVariables = null;
    }
}