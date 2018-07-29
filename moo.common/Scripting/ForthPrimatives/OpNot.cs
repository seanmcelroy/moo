using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpNot
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        NOT ( x -- i ) 

        Returns true (1) if the top stack item is considered false. Returns false (0) otherwise. The stack item can be of any type. For the various types, here are their false values:
            Integer      0
            Float        0.0
            DBRef        #-1
            String       ""
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "NOT requires at least one parameter on the stack");

        var n1 = stack.Pop();
        stack.Push(new ForthDatum(n1.isFalse() ? 1 : 0));
        return default(ForthProgramResult);
    }
}