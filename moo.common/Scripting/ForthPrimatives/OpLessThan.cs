using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpLessThan
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        < ( n1 n2 -- i ) 

        Compares two numbers and returns 1 if n1 is less than n2, and 0 otherwise.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "< requires at least two parameters on the stack");

        var n2 = stack.Pop();
        if (n2.Type != DatumType.Integer && n2.Type != DatumType.Float)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "< requires the second-to-top parameter on the stack to be a number");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.Integer && n1.Type != DatumType.Float)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "< requires the top parameter on the stack to be a number");

        stack.Push(new ForthDatum(Convert.ToSingle(n1.Value) < Convert.ToSingle(n2.Value) ? 1 : 0));
        return default(ForthProgramResult);
    }
}