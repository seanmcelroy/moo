using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpIsFloat
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        FLOAT? ( ? -- i ) 

        Returns true if the item on the stack is a floating point value.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "FLOAT? requires at least one parameter");

        var n1 = stack.Pop();

        stack.Push(new ForthDatum(n1.Type == DatumType.Float ? 1 : 0));
        return default(ForthProgramResult);
    }
}