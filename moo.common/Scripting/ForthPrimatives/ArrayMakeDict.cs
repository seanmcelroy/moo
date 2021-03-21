using System;
using System.Collections.Generic;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayMakeDict
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_MAKE_DICT ( {@ ?} -- a )

            Creates a dictionary type array from a stackrange of index/value pairs.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_MAKE_DICT requires at LEAST one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_MAKE_DICT requires the top parameter on the stack to be an integer");

            if (parameters.Stack.Count < n1.UnwrapInt() * 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"ARRAY_MAKE_DICT has fewer than requested {n1.UnwrapInt()} index/value pairs on the stack");

            var arrayList = new List<ForthDatum>(parameters.Stack.Count);
            for (int i = 0; i < n1.UnwrapInt(); i++)
            {
                var val = parameters.Stack.Pop();
                var idx = parameters.Stack.Pop();
                switch (val.Type)
                {
                    case DatumType.DbRef:
                        arrayList.Add(new ForthDatum(val.UnwrapDbref(), key: idx.Value?.ToString()));
                        break;
                    case DatumType.Integer:
                        arrayList.Add(new ForthDatum(val.UnwrapInt(), key: idx.Value?.ToString()));
                        break;
                    case DatumType.String:
                        arrayList.Add(new ForthDatum(val.Value?.ToString(), key: idx.Value?.ToString()));
                        break;
                    default:
                        throw new InvalidOperationException();
                }
            }

            arrayList.Reverse();
            parameters.Stack.Push(new ForthDatum(arrayList.ToArray()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}