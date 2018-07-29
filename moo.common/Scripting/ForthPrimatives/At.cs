using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class At
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack, Dictionary<string, object> variables)
    {
        /*
        @ ( v -- x ) 
        Retrieves variable v's value x.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "@ requires at least one parameter on the stack");

        var si = stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "LREVERSE requires the top parameter on the stack to be an integer");

        int i = (int)si.Value;
        if (i < 1)
            return new ForthProgramResult(ForthProgramErrorResult.INVALID_VALUE, "LREVERSE requires the top parameter to be greater than or equal to 1");

        if (stack.Count < i)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"LREVERSE would reverse the top {Math.Abs(i)} items from the top of the stack, but only {stack.Count} were present.");

        var temp = new Queue<ForthDatum>();
        for (int n = 0; n < i; n++)
        {
            temp.Enqueue(stack.Pop());
        }

        while (temp.Count > 0)
            stack.Push(temp.Dequeue());

        stack.Push(si);

        return default(ForthProgramResult);
    }
}