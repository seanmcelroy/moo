using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthPrimativeResult;

public static class TimeSplit
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        TIMESPLIT ( i -- is im ih id im iy iw iyd ) 

        Splits a systime value into 8 values in the following order: 
        seconds, minutes, hours, monthday, month, year, weekday, yearday.
        Weekday starts with sunday as 1, and yearday is the day of the year (1-366).
        */
        if (parameters.Stack.Count < 1)
            return new ForthPrimativeResult(ForthErrorResult.STACK_UNDERFLOW, "TIMESPLIT requires one parameter");

        var si = parameters.Stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthPrimativeResult(ForthErrorResult.TYPE_MISMATCH, "TIMESPLIT requires the top parameter on the stack to be an integer");

        var offset = DateTimeOffset.FromUnixTimeSeconds(si.UnwrapInt());
        var dt = offset.DateTime;

        parameters.Stack.Push(new ForthDatum(offset.DateTime.Second));
        parameters.Stack.Push(new ForthDatum(offset.DateTime.Minute));
        parameters.Stack.Push(new ForthDatum(offset.DateTime.Hour));
        parameters.Stack.Push(new ForthDatum(offset.DateTime.Day));
        parameters.Stack.Push(new ForthDatum(offset.DateTime.Month));
        parameters.Stack.Push(new ForthDatum(offset.DateTime.Year));
        parameters.Stack.Push(new ForthDatum(((int)offset.DateTime.DayOfWeek) + 1));
        parameters.Stack.Push(new ForthDatum(offset.DateTime.DayOfYear));

        return ForthPrimativeResult.SUCCESS;
    }
}