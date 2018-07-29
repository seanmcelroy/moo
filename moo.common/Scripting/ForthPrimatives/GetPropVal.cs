using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class GetPropVal
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        GETPROPVAL ( d s -- i ) 

        s must be a string. Retrieves the integer value i associated with property s in object d. If the property is cleared, 0 is returned.
        */
        if (stack.Count < 2)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "GETPROPVAL requires two parameters");

        var n2 = stack.Pop();
        var n1 = stack.Pop();

        return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "+ expects integers or floating point numbers, or no more than one dbref and an integer");

        // TODO: We do not support variable numbers today.  They're depreciated anyway.
    }
}