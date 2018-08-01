using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ForthDatum;
using static ForthProgramResult;

public static class RInstr
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        RINSTR ( s s1 -- i ) 

        Returns the last occurrence of string s1 in string s, or 0 if s1 is not found. '"abcbcba" "bc" rinstr' returns 4.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "RINSTR requires two parameters");

        var n2 = stack.Pop();
        if (n2.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "RINSTR requires the second-to-top parameter on the stack to be a string");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "RINSTR requires the top parameter on the stack to be a string");

        var s1 = (string)n1.Value;
        var s2 = (string)n2.Value;

        stack.Push(new ForthDatum(s1.LastIndexOf(s2) + 1));
        return default(ForthProgramResult);
    }
}