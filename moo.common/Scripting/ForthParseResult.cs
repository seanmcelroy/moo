using System.Collections.Generic;

public struct ForthParseResult
{
    public bool isSuccessful;
    public string reason;

    public ForthParseResult(bool isSuccessful, string reason = null)
    {
        this.isSuccessful = isSuccessful;
        this.reason = reason;
    }
}