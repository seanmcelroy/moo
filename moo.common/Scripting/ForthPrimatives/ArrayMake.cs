using System.Collections.Generic;
using moo.common.Scripting;

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
            if (!parameters.Stack.TryPopStackRange(out List<ForthDatum>? stackrange, out ForthErrorResult? err))
                return new ForthPrimativeResult(err.Value, "ARRAY_MAKE requires the top parameter(s) to be a stackrange (an integer followed by that many items)");
            
            stackrange.Reverse();

            var list = new ForthListArray(stackrange);
            parameters.Stack.Push(new ForthDatum(list));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}