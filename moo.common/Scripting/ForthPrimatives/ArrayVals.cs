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

            if (n1.Value == null)
            {
                parameters.Stack.Push(new ForthDatum(0));
                return ForthPrimativeResult.SUCCESS;
            }

            var values = n1.Value switch
            {
                ForthDictionaryArray da => da.Values,
                ForthListArray la => la,
                _ => [],
            };

            foreach (var value in values)
                parameters.Stack.Push(value);
            parameters.Stack.Push(new ForthDatum(values.Count()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}