using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpNot
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        NOT ( x -- i ) 

        Returns true (1) if the top stack item is considered false. Returns false (0) otherwise. The stack item can be of any type. For the various types, here are their false values:
            Integer      0
            Float        0.0
            DBRef        #-1
            String       ""
        */
        if (parameters.Stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "NOT requires at least one parameter");

        var n1 = parameters.Stack.Pop();
        parameters.Stack.Push(new ForthDatum(n1.isFalse() ? 1 : 0));
        return default(ForthProgramResult);
    }
}