using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class AtoI
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        ATOI ( s -- i ) 

        Turns string s into integer i. If s is not a string, then 0 is pushed onto the stack.
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "ATOI requires one parameter");

        var n1 = stack.Pop();
        if (n1.Type != DatumType.String)
            stack.Push(new ForthDatum(0));
        else
        {
            int i;
            if (int.TryParse((string)n1.Value, out i))
                stack.Push(new ForthDatum(i));
            else
                stack.Push(new ForthDatum(0));
        }

        return default(ForthProgramResult);
    }
}