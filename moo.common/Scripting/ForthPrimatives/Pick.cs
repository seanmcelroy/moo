using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class Pick
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        PICK ( ni ... n1 i -- ni ... n1 ni ) 
        Takes the i'th thing from the top of the stack and pushes it on the top. 1 pick is equivalent to dup, and 2 pick is equivalent to over.
        */
        if (parameters.Stack.Count == 0)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "PICK requires at least one parameter");

        var si = parameters.Stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "PICK requires the top parameter on the stack to be an integer");

        int i = si.UnwrapInt();
        if (i < 1)
            return new ForthPrimativeResult(ForthErrorResult.INVALID_VALUE, "PICK requires the top parameter to be greater than or equal to 1");

        // Shortcut for DUP.
        if (i == 1) {
            if (parameters.Stack.Count == 0)
                return ForthPrimativeResult.SUCCESS;

            parameters.Stack.Push(parameters.Stack.Peek());
            return ForthPrimativeResult.SUCCESS;
        }

        if (parameters.Stack.Count < i)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"PICK would have the {Math.Abs(i)}th item from the top of the stack, but only {parameters.Stack.Count} were present.");

        var ni = parameters.Stack.Skip(i - 1).Take(1).Single();
        parameters.Stack.Push(ni);

        return ForthPrimativeResult.SUCCESS;
    }
}