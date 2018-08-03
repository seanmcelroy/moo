using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpIsString
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        STRING? ( x -- i ) 

        Returns true if x is a string.
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "STRING? requires at least one parameter");

        var n1 = parameters.Stack.Pop();

        parameters.Stack.Push(new ForthDatum(n1.Type == DatumType.String ? 1 : 0));
        return default(ForthProgramResult);
    }
}