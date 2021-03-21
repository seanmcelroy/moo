using System.Linq;

using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayVals
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_VALS ( a -- {?} )

            Returns the values of an array in a stackrange. Example:

            "value0" "value1" 2
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_VALS requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Array)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_VALS requires the top parameter on the stack to be an array");

            var array = n1.UnwrapArray();
            foreach (var elem in array)
                parameters.Stack.Push(elem);
            parameters.Stack.Push(new ForthDatum(array.Length));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}