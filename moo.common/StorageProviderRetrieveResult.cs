public struct StorageProviderRetrieveResult
{
    public bool isSuccess;

    public int id;

    public string serialized;

    public string type;

    public string reason;

    public StorageProviderRetrieveResult(int id, string type, string serialized) {
        this.isSuccess = true;
        this.id = id;
        this.type = type;
        this.serialized = serialized;
        this.reason = null;
    }

    public StorageProviderRetrieveResult(string reason) {
        this.isSuccess = false;
        this.id = -1;
        this.type = null;
        this.serialized = null;
        this.reason = reason;
    }
}