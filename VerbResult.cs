public struct VerbResult {
    bool isSuccess;

    string reason;

    public VerbResult(bool isSuccess, string reason) {
        this.isSuccess =isSuccess;
        this.reason = reason;
    }

    public static implicit operator bool(VerbResult verbResult) {
        return verbResult.isSuccess;
    }
}