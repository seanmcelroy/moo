using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class DupN
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        // DUPN ( ?n..?1 i -- ?n..?1 ?n..?1 ) 
        // Duplicates the top i stack items.
        if (stack.Count == 0)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DUPN requires at least one parameter");

        var si = stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "DUPN requires the top parameter on the stack to be an integer");

        int i = (int)si.Value;
        if (stack.Count < i)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"DUPN would have duplicated {i} items on the stack, but only {stack.Count} were present.");

        foreach (var source in stack.Reverse().Take(i))
            stack.Push(source);

        return default(ForthProgramResult);
    }
}