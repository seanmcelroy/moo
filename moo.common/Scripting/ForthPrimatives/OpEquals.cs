using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class OpEquals
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        = ( n1 n2 -- i ) 

        Compares two numbers and returns 1 if n1 is equal to n2, and 0 otherwise.
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "= requires two parameters");

        var n2 = parameters.Stack.Pop();
        if (n2.Type != DatumType.Integer && n2.Type != DatumType.Float)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "= requires the second-to-top parameter on the stack to be a number");

        var n1 = parameters.Stack.Pop();
        if (n1.Type != DatumType.Integer && n1.Type != DatumType.Float)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "= requires the top parameter on the stack to be a number");

        parameters.Stack.Push(new ForthDatum(Convert.ToSingle(n1.Value) == Convert.ToSingle(n2.Value) ? 1 : 0));
        return ForthPrimativeResult.SUCCESS;
    }
}