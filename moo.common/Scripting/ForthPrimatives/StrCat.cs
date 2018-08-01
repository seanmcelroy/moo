using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class StrCat
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        STRCAT ( s1 s2 -- s ) 

        Concatenates two strings s1 and s2 and pushes the result s = s1s2 onto the stack.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "STRCAT requires two parameters");

        var n2 = stack.Pop();
        if (n2.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRCAT requires the second-to-top parameter on the stack to be a string");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "STRCAT requires the top parameter on the stack to be a string");

        stack.Push(new ForthDatum((string)n1.Value + (string)n2.Value));
        return default(ForthProgramResult);
    }
}