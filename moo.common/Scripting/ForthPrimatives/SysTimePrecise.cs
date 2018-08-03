using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class SysTimePrecise
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        SYSTIME_PRECISE ( -- f ) 

        Returns the number of seconds from Jan 1, 1970 GMT as a floating point number, with microsecond accuracy.
        */
        var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        var secondsSinceEpoch = t.TotalMilliseconds / 1000;
        parameters.Stack.Push(new ForthDatum(Convert.ToSingle(secondsSinceEpoch)));

        return default(ForthProgramResult);
    }
}