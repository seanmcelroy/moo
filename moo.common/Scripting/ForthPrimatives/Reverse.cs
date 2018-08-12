using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class Reverse
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        REVERSE ( ?n..?1 i -- ?1..?n ) 

        Reverses the order of the top i items on the stack. Example:
            "a"  "b"  "c"  "d"  "e"  4  reverse
        would return on the stack:
            "a"  "e"  "d"  "c"  "b"
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "REVERSE requires at least one parameter");

        var si = parameters.Stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "REVERSE requires the top parameter on the stack to be an integer");

        int i = si.UnwrapInt();
        if (i < 1)
            return new ForthPrimativeResult(ForthErrorResult.INVALID_VALUE, "REVERSE requires the top parameter to be greater than or equal to 1");

        if (parameters.Stack.Count < i)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"REVERSE would reverse the top {Math.Abs(i)} items from the top of the stack, but only {parameters.Stack.Count} were present.");

        var temp = new Queue<ForthDatum>();
        for (int n = 0; n < i; n++)
        {
            temp.Enqueue(parameters.Stack.Pop());
        }

        while (temp.Count > 0)
            parameters.Stack.Push(temp.Dequeue());

        return ForthPrimativeResult.SUCCESS;
    }
}