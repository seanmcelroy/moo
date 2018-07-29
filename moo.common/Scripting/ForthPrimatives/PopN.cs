using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class PopN
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        // POPN ( ?n..?1 i -- ) 
        // Pops the top i stack items.
        if (stack.Count == 0)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "POPN requires at least one parameter");

        var si = stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "POPN requires the top parameter on the stack to be an integer");

        int i = (int)si.Value;
        if (stack.Count < i)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"POPN would have removed {i} items on the stack, but only {stack.Count} were present.");

        for (int n = 0; n < i; n++)
            stack.Pop();

        return default(ForthProgramResult);
    }
}