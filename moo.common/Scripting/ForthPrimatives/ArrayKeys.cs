using System;
using System.Linq;

using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayKeys
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_KEYS ( a -- {@} )
            Returns the keys of an array in a stackrange. Example:

            "index0" "index1" 2
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_KEYS requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Array)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_KEYS requires the top parameter on the stack to be an array");

            if (n1.Value == null)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            var keys = n1.Value switch
            {
                ForthDictionaryArray da => da.Keys,
                ForthListArray la => la.Select((item, index) => index).Cast<object>(),
                _ => [],
            };

            foreach (var key in keys)
            {
                ForthDatum datum;
                if (key is int ik)
                {
                    datum = new ForthDatum(ik, DatumType.Integer);
                }
                else if (key is string sk)
                {
                    datum = new ForthDatum(sk, DatumType.String);
                }
                else
                {
                    return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, $"ARRAY_KEYS value '{key}' was not an index (integer or string)");
                }
                
                parameters.Stack.Push(datum);
            }
            parameters.Stack.Push(new ForthDatum(keys.Count()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}