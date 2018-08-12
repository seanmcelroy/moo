using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class DupN
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        // DUPN ( ?n..?1 i -- ?n..?1 ?n..?1 ) 
        // Duplicates the top i stack items.
        if (parameters.Stack.Count == 0)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "DUPN requires at least one parameter");

        var si = parameters.Stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "DUPN requires the top parameter on the stack to be an integer");

        int i = si.UnwrapInt();
        if (parameters.Stack.Count < i)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"DUPN would have duplicated {i} items on the stack, but only {parameters.Stack.Count} were present.");

        foreach (var source in parameters.Stack.Reverse().Take(i))
            parameters.Stack.Push(source);

        return ForthPrimativeResult.SUCCESS;
    }
}