using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Swap
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        // SWAP ( x y -- y x ) 
        // Takes objects x and y on the stack and reverses their order.
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "SWAP requires at least two parameters on the stack");

        var y = stack.Pop();
        var x = stack.Pop();
        stack.Push(y);
        stack.Push(x);

        return default(ForthProgramResult);
    }
}