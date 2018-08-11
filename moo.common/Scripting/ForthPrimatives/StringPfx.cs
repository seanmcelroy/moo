using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static ForthDatum;
using static ForthPrimativeResult;

public static class StringPfx
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        STRINGPFX (s s2 -- i) 

        Returns 1 if string s2 is a prefix of string s. If s2 is NOT a prefix of s, then it returns 0. Case insensitive.
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "STRINGPFX requires two parameters");

        var n2 = parameters.Stack.Pop();
        if (n2.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRINGPFX requires the second-to-top parameter on the stack to be a string");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "STRINGPFX requires the top parameter on the stack to be a string");

        var s1 = (string)n1.Value;
        var s2 = (string)n2.Value;

        parameters.Stack.Push(new ForthDatum(s1.StartsWith(s2) ? 1 : 0));
        return ForthPrimativeResult.SUCCESS;
    }
}