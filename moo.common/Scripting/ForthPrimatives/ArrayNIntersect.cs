using System.Collections.Generic;
using System.Linq;

namespace moo.common.Scripting.ForthPrimatives
{
    public static class ArrayNIntersect
    {
        public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
        {
            /*
            array_nintersect ( {a} -- a )

            Returns an array containing the intersection of all the given arrays in the stackrange.
            ie: Only values contained in ALL the given arrays will be returned.
            Multiple arrays are consecutively processed against the results of the previous intersection,
            from the top of the stack down. Duplicate values will appear only once in the returned list.
            Keys will be discarded. Ordering is not preserved. 
            */
            if (!parameters.Stack.TryPopStackRange(out List<ForthDatum>? stackrange, out ForthErrorResult? err))
                return new ForthPrimativeResult(err.Value, "ARRAY_NINTERSECT requires the top parameter(s) to be a stackrange (an integer followed by that many items)");

            var intersectedArray = new ForthListArray(stackrange.Distinct());
            parameters.Stack.Push(new ForthDatum(intersectedArray));
            return ForthPrimativeResult.SUCCESS;
        }
    }
}