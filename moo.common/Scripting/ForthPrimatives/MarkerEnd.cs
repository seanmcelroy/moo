using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class MarkerEnd
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        } ( marker ?n ... ?1 -- ?n ... ?1 i ) 

        Finds the topmost marker in the stack, and counts how many stack items are between it and the top of the stack. The marker is removed from the stack, and the count is pushed onto the stack.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "} requires at least one parameter");

        var temp = new Stack<ForthDatum>();
        bool found = false;
        while (parameters.Stack.Count > 0)
        {
            var n = parameters.Stack.Pop();
            if (n.Type == DatumType.Marker)
            {
                found = true;
                break;
            }
            temp.Push(n);
        }

        if (!found)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "} found no opening { on the stack.");

        int count = temp.Count;

        while (temp.Count > 0)
            parameters.Stack.Push(temp.Pop());

        parameters.Stack.Push(new ForthDatum(count));

        return ForthPrimativeResult.SUCCESS;
    }
}