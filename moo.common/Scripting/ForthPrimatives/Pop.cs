using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Pop
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        // POP ( x -- ) 
        // Pops the top of the stack into oblivion.
        if (stack.Count == 0)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "POP requires at least one item on the stack, but the stack is empty.");

        stack.Pop();

        return default(ForthProgramResult);
    }
}