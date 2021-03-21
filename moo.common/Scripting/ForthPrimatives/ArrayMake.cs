using System.Collections.Generic;
using static moo.common.Scripting.ForthDatum;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayMake
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            ARRAY_MAKE ( {?} -- a )

            Creates a list type array from a stackrange.
            */
            if (parameters.Stack.Count < 1)
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ARRAY_MAKE requires at LEAST one parameter");

            var n1 = parameters.Stack.Pop();
            if (n1.Type != DatumType.Integer)
                return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ARRAY_MAKE requires the top parameter on the stack to be an integer");

            if (parameters.Stack.Count < n1.UnwrapInt())
                return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"ARRAY_MAKE has fewer than requested {n1.UnwrapInt()} items on the stack");

            var arrayList = new List<ForthDatum>(parameters.Stack.Count);
            for (int i = 0; i < n1.UnwrapInt(); i++)
                arrayList.Add(parameters.Stack.Pop());

            parameters.Stack.Push(new ForthDatum(arrayList.ToArray()));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}