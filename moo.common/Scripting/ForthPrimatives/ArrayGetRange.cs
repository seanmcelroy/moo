using System;
using System.Collections.Generic;
using System.Linq;

using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayGetRange
    {
        private class ArrayIndexComparer : IComparer<object>
        {
            public int Compare(object? x, object? y)
            {
                if (x == null && y == null)
                    return 0;
                if (x == null)
                    return 1;
                if (y == null)
                    return -1;
                if (x is int xi && y is int yi)
                    return xi.CompareTo(yi);
                if (x is string xs && y is string ys)
                    return xs.CompareTo(ys);
                if (x is int)
                    return -1;
                if (y is int)
                    return 1;

                throw new NotImplementedException($"One of the array indexes is not of the required integer or string type ({x.GetType().Name} vs. {y.GetType().Name})");
            }
        }

        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_GETRANGE ( a @ @ -- a' )

            Returns as an array the range between two indexes (inclusive) from an array. 
            */
            if (parameters.Stack.Count < 3)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_GETRANGE requires three parameters");

            if (!parameters.Stack.TryPopArrayIndex(out object? index2, out ForthErrorResult? err3))
                return new ForthPrimativeResult(err3.Value, "ARRAY_GETRANGE requires the top parameter(s) to be an array index (an integer or string)");

            if (!parameters.Stack.TryPopArrayIndex(out object? index1, out ForthErrorResult? err2))
                return new ForthPrimativeResult(err2.Value, "ARRAY_GETRANGE requires the second-top parameter(s) to be an array index (an integer or string)");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Array)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_GETRANGE requires the third-top parameter on the stack to be an array");

            if (n1.Value == null)
            {
                parameters.Stack.Push(new ForthDatum(ForthListArray.EMPTY));
                return ForthPrimativeResult.SUCCESS;
            }

            if (n1.Value is ForthDictionaryArray da)
            {
                if (da.Count == 0)
                {
                    parameters.Stack.Push(new ForthDatum(ForthListArray.EMPTY));
                    return ForthPrimativeResult.SUCCESS;
                }

                var comparer = new ArrayIndexComparer();
                var sortedKeys = da.Keys.Order(comparer).ToArray();
                var minKey = sortedKeys.FirstOrDefault((k) => comparer.Compare(k, index1) >= 0);
                var maxKey = sortedKeys.LastOrDefault((k) => comparer.Compare(k, index2) <= 0);
                var selectedKeys = sortedKeys.Where(k => comparer.Compare(k, minKey) >= 0 && comparer.Compare(k, maxKey) <= 0);
                var selectedKvps = selectedKeys.ToDictionary(k => k, v => da[v]);

                parameters.Stack.Push(new ForthDatum(new ForthDictionaryArray(selectedKvps)));
                return ForthPrimativeResult.SUCCESS;
            }

            if (n1.Value is ForthListArray la)
            {
                if (la.Count == 0)
                {
                    parameters.Stack.Push(new ForthDatum(ForthListArray.EMPTY));
                    return ForthPrimativeResult.SUCCESS;
                }

                if (index1 is not int i1)
                    return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, $"ARRAY_GETRANGE requires integer indexes for list arrays, but received '{index1}'");
                if (index2 is not int i2)
                    return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, $"ARRAY_GETRANGE requires integer indexes for list arrays, but received '{index2}'");
                if (i1 > i2)
                {
                    parameters.Stack.Push(new ForthDatum(ForthListArray.EMPTY));
                    return ForthPrimativeResult.SUCCESS;
                }

                // Clamp
                i1 = Math.Max(i1, 0);
                i2 = Math.Min(i2, la.Count - 1);

                if (i1 > la.Count - 1)
                {
                    parameters.Stack.Push(new ForthDatum(ForthListArray.EMPTY));
                    return ForthPrimativeResult.SUCCESS;
                }

                parameters.Stack.Push(new ForthDatum(new ForthListArray(la.ToArray()[i1..(i2 + 1)])));
                return ForthPrimativeResult.SUCCESS;
            }

            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, $"ARRAY_GETRANGE recieved an unsupported array type");
        }
    }
}