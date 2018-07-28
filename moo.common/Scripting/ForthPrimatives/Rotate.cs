using System;
using System.Collections.Generic;
using static ForthDatum;
using static ForthProgramResult;

public static class Rotate
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
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
        if (stack.Count == 0)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "ROTATE requires at least one parameter on the stack");

        var si = stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "ROTATE requires the top parameter on the stack to be an integer");

        int i = (int)si.Value;
        if (stack.Count < i)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, $"ROTATE would have rotated {Math.Abs(i)} items on the stack, but only {stack.Count} were present.");

        var data = new ForthDatum[Math.Abs(i)];
        for (int n = Math.Abs(i) - 1; n >= 0; n--)
            data[n] = stack.Pop();

        if (i > 0) {
            for (int n = 0; n < data.Length; n++)
                stack.Push(data[(n + 1) % (data.Length)]);
        }
        else {
             for (int n = 0; n < data.Length; n++)
                stack.Push(data[(n == 0 ? data.Length : n) - 1]);
        }

        //var shift = i < 0 ? -1 : 1;
        //for (int n = 0; n < data.Length; n++)
        //    stack.Push(data[Math.Abs((n + shift) % (data.Length))]);

        return default(ForthProgramResult);
    }
}