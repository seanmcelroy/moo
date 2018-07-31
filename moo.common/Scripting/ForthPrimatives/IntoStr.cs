using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class IntoStr
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        INTOSTR ( x -- s ) 

        x must be an integer or a dbref. Converts x into string s.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "INTOSTR requires one parameter");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.Integer && n1.Type != DatumType.DbRef)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "< requires the top parameter on the stack to be a number");

        stack.Push(new ForthDatum(n1.Value.ToString()));
        return default(ForthProgramResult);
    }
}