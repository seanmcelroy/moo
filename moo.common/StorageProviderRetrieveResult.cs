public struct StorageProviderRetrieveResult
{
    public bool isSuccess;

    public Dbref id;

    public string? serialized;

    public string? type;

    public string? reason;

    public StorageProviderRetrieveResult(Dbref id, string type, string serialized)
    {
        this.isSuccess = true;
        this.id = id;
        this.type = type;
        this.serialized = serialized;
        this.reason = null;
    }

    public StorageProviderRetrieveResult(string reason)
    {
        this.isSuccess = false;
        this.id = Dbref.NOT_FOUND;
        this.type = null;
        this.serialized = null;
        this.reason = reason;
    }
}