using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class LReverse
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        REVERSE ( ?n..?1 i -- ?1..?n ) 

        Reverses the order of the top i items on the stack. Example:
            "a"  "b"  "c"  "d"  "e"  4  reverse
        would return on the stack:
            "a"  "e"  "d"  "c"  "b"
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "LREVERSE requires at least one parameter");

        var si = parameters.Stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "LREVERSE requires the top parameter to be an integer");

        int i = (int)si.Value;
        if (i < 1)
            return new ForthProgramResult(ForthProgramErrorResult.INVALID_VALUE, "LREVERSE requires the top parameter to be greater than or equal to 1");

        if (parameters.Stack.Count < i)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"LREVERSE would reverse the top {Math.Abs(i)} items from the top of the stack, but only {parameters.Stack.Count} were present.");

        var temp = new Queue<ForthDatum>();
        for (int n = 0; n < i; n++)
        {
            temp.Enqueue(parameters.Stack.Pop());
        }

        while (temp.Count > 0)
            parameters.Stack.Push(temp.Dequeue());

        parameters.Stack.Push(si);

        return default(ForthProgramResult);
    }
}