using System;
using static ForthDatum;

public static class Abs
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        ABS ( i -- i )

        Given an integer, returns its absolute value. 
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ABS requires one parameter");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ABS requires the top parameter on the stack to be an integer");

        parameters.Stack.Push(new ForthDatum(Math.Abs(n1.UnwrapInt())));
        return ForthPrimativeResult.SUCCESS;
    }
}