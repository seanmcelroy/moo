using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Put
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        PUT ( nx...n1 ni i -- nx...ni...n1 ) 

        Replaces the i'th item from the top of the stack with the value of ni. The command sequence '1 put' is equivalent to 'swap pop'. Example:
            "a"  "b"  "c"  "d"  "e"  3  put
        would return on the stack:
            "a"  "e"  "c"  "d"
        */
        if (stack.Count < 4)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "PUT requires at least four parameters on the stack");

        var si = stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "PUT requires the top parameter on the stack to be an integer");

        int i = (int)si.Value;
        if (i < 1)
            return new ForthProgramResult(ForthProgramErrorResult.INVALID_VALUE, "PUT requires the top parameter to be greater than or equal to 1");

        if (stack.Count < i)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"PUT would grab the {Math.Abs(i)}th item from the top of the stack, but only {stack.Count} were present.");

        var ni = stack.Pop();

        var temp = new Stack<ForthDatum>(i);
        for (int n = 0; n < i; n++) {
            temp.Push(stack.Pop());
        }

        stack.Push(ni);
        temp.Pop();

        while (temp.Count > 0)
            stack.Push(temp.Pop());
            
        return default(ForthProgramResult);
    }
}