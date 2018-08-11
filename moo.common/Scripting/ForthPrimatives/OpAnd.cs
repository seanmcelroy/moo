using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class OpAnd
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        AND ( x1 x2 -- i ) 

        Returns true (1) if both of the top two stack items are considered true. Returns false (0) otherwise. The stack items can be of any type. For the various types, here are their false values:
            Integer      0
            Float        0.0
            DBRef        #-1
            String       ""
        */
        if (parameters.Stack.Count < 2)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "AND requires two parameters");

        var n1 = parameters.Stack.Pop();
        var n2 = parameters.Stack.Pop();

        parameters.Stack.Push(new ForthDatum(n1.isTrue() && n2.isTrue() ? 1 : 0));
        return ForthPrimativeResult.SUCCESS;
    }
}