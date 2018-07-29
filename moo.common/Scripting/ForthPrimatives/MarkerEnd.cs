using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class MarkerEnd
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        } ( marker ?n ... ?1 -- ?n ... ?1 i ) 

        Finds the topmost marker in the stack, and counts how many stack items are between it and the top of the stack. The marker is removed from the stack, and the count is pushed onto the stack.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "} requires at least one parameter");

        var temp = new Stack<ForthDatum>();
        bool found = false;
        while (stack.Count > 0)
        {
            var n = stack.Pop();
            if (n.Type == DatumType.Marker)
            {
                found = true;
                break;
            }
            temp.Push(n);
        }

        if (!found)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "} found no opening { on the stack.");

        int count = temp.Count;

        while (temp.Count > 0)
            stack.Push(temp.Pop());

        stack.Push(new ForthDatum(count));

        return default(ForthProgramResult);
    }
}