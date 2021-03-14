using System;
using System.Diagnostics.CodeAnalysis;
using moo.common.Database;
using Newtonsoft.Json;

namespace moo.common.Models
{
    [JsonConverter(typeof(LockSerializer))]
    public struct Lock
    {
        private string raw;

        public Lock(string raw) => this.raw = raw;

        public static bool TryParse(string s, out Lock result)
        {
            result = new Lock
            {
                raw = s
            };
            return true;
        }

        public override int GetHashCode() => raw.GetHashCode();

        public bool Equals(Lock obj) => string.Equals(obj.raw, this.raw, StringComparison.Ordinal);

        public override bool Equals(object? obj)
        {
            if (!(obj is Lock))
                return false;

            return this.Equals((Lock)obj);
        }

        public override string ToString() => raw;

        public static bool operator ==(Lock left, Lock right) => left.Equals(right);

        public static bool operator !=(Lock left, Lock right) => !(left == right);
    }
}