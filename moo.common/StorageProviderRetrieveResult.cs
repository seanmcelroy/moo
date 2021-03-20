using System;
using System.Collections.Generic;
using moo.common.Models;

namespace moo.common
{
    public struct StorageProviderRetrieveResult
    {
        public bool isSuccess;

        public Dbref id;

        public string? serialized;

        public string? type;

        public string? name;

        public string? reason;

        public StorageProviderRetrieveResult(Dbref id, string type, string? name, string serialized)
        {
            isSuccess = true;
            this.id = id;
            this.type = type;
            this.name = name;
            this.serialized = serialized;
            reason = null;
        }

        public StorageProviderRetrieveResult(string reason)
        {
            isSuccess = false;
            id = Dbref.NOT_FOUND;
            type = null;
            name = null;
            serialized = null;
            this.reason = reason;
        }

        public override bool Equals(object? obj) => obj is StorageProviderRetrieveResult result &&
                   isSuccess == result.isSuccess &&
                   EqualityComparer<Dbref>.Default.Equals(id, result.id) &&
                   string.CompareOrdinal(serialized, result.serialized) == 0 &&
                   string.CompareOrdinal(type, result.type) == 0 &&
                   string.CompareOrdinal(name, result.name) == 0 &&
                   reason == result.reason;

        public override int GetHashCode() => HashCode.Combine(id, serialized, type, name, reason, isSuccess);

        public static bool operator ==(StorageProviderRetrieveResult left, StorageProviderRetrieveResult right) => left.Equals(right);

        public static bool operator !=(StorageProviderRetrieveResult left, StorageProviderRetrieveResult right) => !(left == right);
    }
}