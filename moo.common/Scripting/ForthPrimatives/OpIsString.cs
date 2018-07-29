using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpIsString
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        STRING? ( x -- i ) 

        Returns true if x is a string.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "STRING? requires at least one parameter on the stack");

        var n1 = stack.Pop();

        stack.Push(new ForthDatum(n1.Type == DatumType.String ? 1 : 0));
        return default(ForthProgramResult);
    }
}