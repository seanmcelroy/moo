using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Reverse
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        REVERSE ( ?n..?1 i -- ?1..?n ) 

        Reverses the order of the top i items on the stack. Example:
            "a"  "b"  "c"  "d"  "e"  4  reverse
        would return on the stack:
            "a"  "e"  "d"  "c"  "b"
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "REVERSE requires at least one parameter");

        var si = stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "REVERSE requires the top parameter on the stack to be an integer");

        int i = (int)si.Value;
        if (i < 1)
            return new ForthProgramResult(ForthProgramErrorResult.INVALID_VALUE, "REVERSE requires the top parameter to be greater than or equal to 1");

        if (stack.Count < i)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"REVERSE would reverse the top {Math.Abs(i)} items from the top of the stack, but only {stack.Count} were present.");

        var temp = new Queue<ForthDatum>();
        for (int n = 0; n < i; n++)
        {
            temp.Enqueue(stack.Pop());
        }

        while (temp.Count > 0)
            stack.Push(temp.Dequeue());

        return default(ForthProgramResult);
    }
}