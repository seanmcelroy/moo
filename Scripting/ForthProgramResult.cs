public struct ForthProgramResult
{

    public enum ForthProgramErrorResult : byte
    {
        INTERRUPTED,
        STACK_UNDERFLOW,
        TYPE_MISMATCH
    }

    public bool isSuccessful;
    public object result;
    public string reason;

    public ForthProgramResult(object result, string reason = null)
    {
        this.isSuccessful = true;
        this.result = result;
        this.reason = reason;
    }

    public ForthProgramResult(ForthProgramErrorResult errorCode, string reason)
    {
        this.isSuccessful = false;
        this.result = errorCode;
        this.reason = reason;
    }
}