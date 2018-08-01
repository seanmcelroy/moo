using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Time
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        TIME ( -- s m h ) 

        Returns the time of day as integers on the stack, seconds, then minutes, then hours.
        */
        var now = DateTime.UtcNow;

        stack.Push(new ForthDatum(now.Second));
        stack.Push(new ForthDatum(now.Minute));
        stack.Push(new ForthDatum(now.Hour));

        return default(ForthProgramResult);
    }
}