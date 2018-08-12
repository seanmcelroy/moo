using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class StripTail
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        STRIPTAIL (s -- s) 

        Strips trailing spaces from the given string.
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "TOLOWER requires one parameter");

        var s = parameters.Stack.Pop();
        if (s.Type != DatumType.String)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "TOLOWER requires the top parameter on the stack to be a string");

        parameters.Stack.Push(new ForthDatum(((string)s.Value).TrimEnd()));
        return ForthPrimativeResult.SUCCESS;
    }
}