using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class MathModulo
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        % ( n1 n2 -- i ) 

        This returns the integer modulo (remainder) of the division of two numbers, n1 % n2.
        Floats cannot use the % modulo function. For them, use either the FMOD or MODF primitives.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "% requires at least two parameters on the stack");

        var n2 = stack.Pop();
        var n1 = stack.Pop();

        if (n2.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "% requires arguments to be integers");

        if (n1.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "% requires arguments to be integers");

        stack.Push(new ForthDatum((int)n1.Value % (int)n2.Value));
        return default(ForthProgramResult);
    }
}