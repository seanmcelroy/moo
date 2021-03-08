using moo.common.Models;

namespace moo.common
{
    public struct StorageProviderRetrieveResult
    {
        public bool isSuccess;

        public Dbref id;

        public string? serialized;

        public string? type;

        public string? reason;

        public StorageProviderRetrieveResult(Dbref id, string type, string serialized)
        {
            isSuccess = true;
            this.id = id;
            this.type = type;
            this.serialized = serialized;
            reason = null;
        }

        public StorageProviderRetrieveResult(string reason)
        {
            isSuccess = false;
            id = Dbref.NOT_FOUND;
            type = null;
            serialized = null;
            this.reason = reason;
        }
    }
}