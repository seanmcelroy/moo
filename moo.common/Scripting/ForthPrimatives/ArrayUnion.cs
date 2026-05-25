using System.Linq;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayUnion
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_UNION ( a1 a2 -- a )

            Returns a list array containing the union of values of both the given arrays.
            ie: If a value is found in either of the given arrays, then it will be returned
            in the result list. Duplicate values will appear only once in the returned list.
            Keys are discarded. Ordering is not preserved. 
            */
            if (parameters.Stack.Count < 2)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_UNION requires two parameters");

            var a2 = parameters.Stack.Pop();
            if (a2.Type != DatumType.Array || a2.Value is not ForthListArray la2)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_UNION requires the top parameter on the stack to be an array");

            var a1 = parameters.Stack.Pop();
            if (a1.Type != DatumType.Array || a1.Value is not ForthListArray la1)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_UNION requires the second-to-top parameter on the stack to be an array");

            var a1u = a1.UnwrapListArray();
            var a2u = a2.UnwrapListArray();
            var unioned = a1u.Union(a2u);
            var unionedArray = new ForthListArray(unioned);
            parameters.Stack.Push(new ForthDatum(unionedArray));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}