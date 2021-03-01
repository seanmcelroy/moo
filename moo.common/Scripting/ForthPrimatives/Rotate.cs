using System;
using static ForthDatum;

public static class Rotate
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        ROTATE ( ni ... n1 i -- n(i-1) ... n1 ni )
        Rotates the top i things on the stack. Using a negative rotational value rotates backwards. Examples:
            "a"  "b"  "c"  "d"  4  rotate
        would leave
            "b"  "c"  "d"  "a"
        on the stack.
            "a"  "b"  "c"  "d"  -4  rotate
        would leave
            "d" "a" "b" "c" on the stack.
        */
        if (parameters.Stack.Count == 0)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "ROTATE requires at least one parameter");

        var si = parameters.Stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "ROTATE requires the top parameter on the stack to be an integer");

        var i = si.UnwrapInt();
        var ai = Math.Abs(i);
        if (parameters.Stack.Count < ai)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, $"ROTATE would have rotated {ai} items on the stack, but only {parameters.Stack.Count} were present.");

        var data = new ForthDatum[ai];
        for (int n = 0; n < ai; n++)
            data[n] = parameters.Stack.Pop();

        if (i > 0)
        {
            for (int n = ai - 2; n >= 0; n--)
                parameters.Stack.Push(data[n]);
            parameters.Stack.Push(data[ai - 1]);
        }
        else
        {
            parameters.Stack.Push(data[0]);
            for (int n = ai - 1; n > 0; n--)
                parameters.Stack.Push(data[n]);
        }

        return ForthPrimativeResult.SUCCESS;
    }
}