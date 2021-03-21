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
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_VALS requires one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Array)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_VALS requires the top parameter on the stack to be an array");

            var array = n1.UnwrapArray();
            for (int i = 0; i < array.Length; i++)
                parameters.Stack.Push(new ForthDatum(array[i].Key ?? $"index{i}"));
            parameters.Stack.Push(new ForthDatum(array.Length));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}