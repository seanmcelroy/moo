using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Swap
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        // SWAP ( x y -- y x ) 
        // Takes objects x and y on the stack and reverses their order.
        if (parameters.Stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "SWAP requires two parameters");

        var y = parameters.Stack.Pop();
        var x = parameters.Stack.Pop();
        parameters.Stack.Push(y);
        parameters.Stack.Push(x);

        return ForthProgramResult.SUCCESS;
    }
}