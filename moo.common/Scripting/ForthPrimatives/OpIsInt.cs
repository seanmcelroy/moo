using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpIsInt
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        INT? ( x -- i ) 

        Returns true if x is a int.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "INT? requires at least one parameter");

        var n1 = stack.Pop();

        stack.Push(new ForthDatum(n1.Type == DatumType.Integer ? 1 : 0));
        return default(ForthProgramResult);
    }
}