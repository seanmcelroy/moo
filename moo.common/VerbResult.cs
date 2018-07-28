public struct VerbResult
{
    public bool isSuccess;

    public string reason;

    public VerbResult(bool isSuccess, string reason)
    {
        if (!isSuccess && reason == null)
            throw new System.ArgumentNullException(nameof(reason));

        this.isSuccess = isSuccess;
        this.reason = reason;
    }

    public static implicit operator bool(VerbResult verbResult)
    {
        return verbResult.isSuccess;
    }
}