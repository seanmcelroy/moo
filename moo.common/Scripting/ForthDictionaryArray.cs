using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using moo.common.Models;

namespace moo.common.Scripting
{
    public readonly record struct ForthDictionaryArray(IEnumerable<KeyValuePair<object, ForthDatum>> Pairs) : IReadOnlyDictionary<object, ForthDatum>, IReadOnlyList<ForthDatum>
    {
        public readonly static ForthDictionaryArray EMPTY = new();

        private readonly Dictionary<object, ForthDatum> inner = Pairs.ToDictionary(k => k.Key, v => v.Value);

        public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out ForthDictionaryArray? result)
        {
            result = null;

            if (string.IsNullOrWhiteSpace(s) 
                || s.Length < 2 
                || s[0] != '[' 
                || s[^1] != ']')
                return false;

            if (s.Length == 2) // []
            {
                result = EMPTY;
                return true;
            }

            List<KeyValuePair<object, ForthDatum>> elements = [];
            foreach (var element in s.Split('\x1e', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Unit Separator control code
            {
                if (!s.Contains('\x1f'))
                {
                    Console.Error.WriteLine($"ERROR: BAD ARRAY: {s}");
                }

                var kvp = element.Split('\x1f', 2);
                if (ForthDatum.TryConvert(kvp[1], out var value))
                    elements.Add(new KeyValuePair<object, ForthDatum>(kvp[0], value.Value));
                else
                    throw new InvalidOperationException($"Cannot infer type of '{value}' as array element at index '{kvp[0]}'");
            }

            result = new ForthDictionaryArray([.. elements]);
            return true;
        }

        public override readonly string ToString()
        {
            if (inner == null || inner.Count == 0)
                return "[]";

            var s = string.Join("\x1e",
                inner.Select(static e =>
                {
                    return $"{SerializeValue(e.Key)}\x1f{SerializeValue(e.Value.Value)}";
                }));

            return $"[{s}]";
        }

        private static string SerializeValue(object value)
        {
            return value switch
            {
                string s => s,
                int i => i.ToString(),
                Dbref d => d.ToString(),
                float f => f.ToString(".0###########"),
                Lock l => l.ToString(),
                ForthDictionaryArray da => da.SerializeNestedArray(),
                ForthListArray la => la.SerializeNestedArray(),
                _ => throw new NotImplementedException($"Unable to serialize value type {value.GetType().Name}"),
            };
        }

        /// <summary>
        /// Array values are stored as base-64 encoded values.
        /// This allows arrays to be recursively nested.
        /// </summary>
        /// <returns></returns>
        internal readonly string SerializeNestedArray() => Convert.ToBase64String(Encoding.UTF8.GetBytes(ToString()));

        public readonly int Count => inner.Count;

        public ForthDatum this[int index] => inner.ElementAt(index).Value;
        public ForthDatum this[object key] => inner[key];

        public readonly bool ContainsKey(object key) => inner.ContainsKey(key);

        readonly IEnumerator<ForthDatum> IEnumerable<ForthDatum>.GetEnumerator() => inner.Values.GetEnumerator();

        readonly bool IReadOnlyDictionary<object, ForthDatum>.ContainsKey(object key) => inner.ContainsKey(key);

        readonly bool IReadOnlyDictionary<object, ForthDatum>.TryGetValue(object key, out ForthDatum value)
        {
            var tgv = inner.TryGetValue(key, out ForthDatum innerValue);
            value = innerValue;
            return tgv;
        }

        readonly IEnumerator IEnumerable.GetEnumerator() => inner.Values.GetEnumerator();

        readonly IEnumerator<KeyValuePair<object, ForthDatum>> IEnumerable<KeyValuePair<object, ForthDatum>>.GetEnumerator() => inner.GetEnumerator();

        readonly IEnumerable<object> IReadOnlyDictionary<object, ForthDatum>.Keys => inner.Keys;

        readonly IEnumerable<ForthDatum> IReadOnlyDictionary<object, ForthDatum>.Values => inner.Values;

        public readonly IEnumerable<object> Keys => inner.Keys;
        public readonly IEnumerable<ForthDatum> Values => inner.Values;

        readonly int IReadOnlyCollection<KeyValuePair<object, ForthDatum>>.Count => inner.Count;

        readonly ForthDatum IReadOnlyDictionary<object, ForthDatum>.this[object key] => inner[key];
    }
}