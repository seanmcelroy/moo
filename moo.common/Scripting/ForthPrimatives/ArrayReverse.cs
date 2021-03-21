using System.Linq;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayReverse
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_REVERSE (a -- a')

            Takes a list array and reverses the order of its elements. 
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_REVERSE requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Array)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_REVERSE requires the top parameter on the stack to be an array");

            var reversed = n1.UnwrapArray().Reverse().ToArray();

            parameters.Stack.Push(new ForthDatum(reversed));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}