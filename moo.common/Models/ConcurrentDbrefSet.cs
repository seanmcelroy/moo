using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text.Json.Serialization;
using moo.common.Database;

namespace moo.common.Models
{
    [JsonConverter(typeof(ConcurrentDbrefSetSerializer))]
    public class ConcurrentDbrefSet
    {
        private readonly ConcurrentDictionary<Dbref, int> store;

        public int Count => store.Count;

        public ConcurrentDbrefSet() => store = new();

        public ConcurrentDbrefSet(IEnumerable<Dbref> dbrefs)
        {
            store = new ConcurrentDictionary<Dbref, int>(dbrefs.Select(d => new KeyValuePair<Dbref, int>(d, 0)));
        }

        public bool TryAdd(Dbref dbref) => store.TryAdd(dbref, 0);
        public bool TryRemove(Dbref dbref) => store.TryRemove(dbref, out _);

        public void Clear() => store.Clear();
        public bool Contains(Dbref dbref) => store.ContainsKey(dbref);

        public override bool Equals(object? obj)
        {
            if (!(obj is ConcurrentDbrefSet))
                return false;

            var cds = (ConcurrentDbrefSet)obj;
            if (cds.Count != Count)
                return false;

            foreach (var pdk in cds.ToImmutableArray())
                if (!store.ContainsKey(pdk))
                    return false;

            return true;
        }

        public ImmutableArray<Dbref> ToImmutableArray() => this.store.Keys.ToImmutableArray();
        public ImmutableList<Dbref> ToImmutableList() => this.store.Keys.ToImmutableList();

        public override int GetHashCode() => store.Select(x => x.GetHashCode()).Aggregate((c, n) => c ^ n);
    }
}