using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayCount
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_COUNT ( a -- i )

            Returns the count of number of items in the array.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_COUNT requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Array)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_COUNT requires the top parameter on the stack to be an array");

            var array = n1.UnwrapArray();

            parameters.Stack.Push(new ForthDatum(array.Length));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}