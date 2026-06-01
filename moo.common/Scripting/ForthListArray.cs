using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using moo.common.Models;

namespace moo.common.Scripting
{
    public readonly record struct ForthListArray(IEnumerable<ForthDatum> Items) : IReadOnlyList<ForthDatum>
    {
        public readonly static ForthListArray EMPTY = new([]);

        private readonly List<ForthDatum> inner = [.. Items];

        public static bool TryParse([NotNullWhen(true)] string? s, [NotNullWhen(true)] out ForthListArray? result)
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

            List<ForthDatum> elements = [];
            foreach (var element in s.Split('\x1e', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)) // Unit Separator control code
            {
                if (ForthDatum.TryConvert(element, out var value))
                    elements.Add(value.Value);
                else
                    throw new InvalidOperationException($"Cannot infer type of '{value}' as array element");
            }

            result = new ForthListArray([.. elements]);
            return true;
        }

        public override readonly string ToString()
        {
            if (inner == null || inner.Count == 0)
                return "[]";

            var s = string.Join("\x1e", inner.Select(static e => $"{SerializeValue(e.Value)}"));

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

        IEnumerator<ForthDatum> IEnumerable<ForthDatum>.GetEnumerator() => inner.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => inner.GetEnumerator();

        public readonly int Count => inner.Count;

        public ForthDatum this[int index] => inner[index];
    }
}