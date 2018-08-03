using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Time
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        TIME ( -- s m h ) 

        Returns the time of day as integers on the stack, seconds, then minutes, then hours.
        */
        var now = DateTime.UtcNow;

        parameters.Stack.Push(new ForthDatum(now.Second));
        parameters.Stack.Push(new ForthDatum(now.Minute));
        parameters.Stack.Push(new ForthDatum(now.Hour));

        return default(ForthProgramResult);
    }
}