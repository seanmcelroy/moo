using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using moo.common.Database;

namespace moo.common.Models
{
    [JsonConverter(typeof(LockSerializer))]
    public struct Lock(string raw) : IEquatable<Lock>
    {
        private string raw = raw;

        public static bool TryParse([NotNullWhen(true)] string s, out Lock? result)
        {
            result = null;
            if (string.IsNullOrWhiteSpace(s))
                return false;

            var lev = LockExpressionValue.Parse(s);
            if (lev.inners == null && string.IsNullOrWhiteSpace(lev.terminal))
                return false;

            result = new Lock
            {
                raw = s
            };
            return true;
        }

        public override readonly int GetHashCode() => raw.GetHashCode();

        public readonly bool Equals(Lock other) => string.Equals(other.raw, raw, StringComparison.Ordinal);

        public readonly bool Equals(Lock? other) => other != null && string.Equals(other.Value.raw, raw, StringComparison.Ordinal);

        public override readonly bool Equals(object? obj) => obj is Lock lk && Equals(lk);

        public override readonly string ToString() => raw;

        public static bool operator ==(Lock left, Lock right) => left.Equals(right);

        public static bool operator !=(Lock left, Lock right) => !(left == right);
    }
}