using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class StrLen
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        STRLEN ( s -- i ) 

        Returns the length of string s.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "STRLEN requires one parameter");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRLEN requires the top parameter on the stack to be a string");

        stack.Push(new ForthDatum(n1.Value == null ? 0 : ((string)n1.Value).Length));

        return default(ForthProgramResult);
    }
}