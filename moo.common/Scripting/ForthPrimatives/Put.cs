using System;
using System.Collections.Generic;
using static ForthDatum;

public static class Put
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        PUT ( nx...n1 ni i -- nx...ni...n1 ) 

        Replaces the i'th item from the top of the stack with the value of ni. The command sequence '1 put' is equivalent to 'swap pop'. Example:
            "a"  "b"  "c"  "d"  "e"  3  put
        would return on the stack:
            "a"  "e"  "c"  "d"
        */
        if (parameters.Stack.Count < 3)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "PUT requires at least three parameters");

        var si = parameters.Stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "PUT requires the top parameter on the stack to be an integer");

        int i = si.UnwrapInt();
        if (i < 1)
            return new ForthPrimativeResult(ForthErrorResult.INVALID_VALUE, "PUT requires the top parameter to be greater than or equal to 1");

        if (parameters.Stack.Count < i)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"PUT would grab the {Math.Abs(i)}th item from the top of the stack, but only {parameters.Stack.Count} were present.");

        var ni = parameters.Stack.Pop();

        var temp = new Stack<ForthDatum>(i);
        for (int n = 0; n < i; n++)
        {
            temp.Push(parameters.Stack.Pop());
        }

        parameters.Stack.Push(ni);
        temp.Pop();

        while (temp.Count > 0)
            parameters.Stack.Push(temp.Pop());

        return ForthPrimativeResult.SUCCESS;
    }
}