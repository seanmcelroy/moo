using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class OpXor
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        XOR ( x1 x2 -- i ) 

        Returns true (1) if either of the top two stack items are considered true, but NOT both of them. Returns false (0) otherwise. The stack items can be of any type. For the various types, here are their false values:
            Integer      0
            Float        0.0
            DBRef        #-1
            String       ""
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "XOR requires at least two parameters on the stack");

        var n1 = stack.Pop();
        var n2 = stack.Pop();

        stack.Push(new ForthDatum(n1.isTrue() ^ n2.isTrue() ? 1 : 0));
        return default(ForthProgramResult);
    }
}