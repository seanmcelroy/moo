using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpIsDbRef
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        DBREF? ( x -- i ) 

        Returns true if x is a dbref.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "DBREF? requires one parameter");

        var n1 = stack.Pop();

        stack.Push(new ForthDatum(n1.Type == DatumType.DbRef ? 1 : 0));
        return default(ForthProgramResult);
    }
}