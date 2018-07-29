using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class TimeSplit
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        TIMESPLIT ( i -- is im ih id im iy iw iyd ) 

        Splits a systime value into 8 values in the following order: 
        seconds, minutes, hours, monthday, month, year, weekday, yearday.
        Weekday starts with sunday as 1, and yearday is the day of the year (1-366).
        */
        if (stack.Count < 1)
            return new ForthProgramResult(ForthProgramErrorResult.STACK_UNDERFLOW, "TIMESPLIT requires one parameter");

        var si = stack.Pop();
        if (si.Type != DatumType.Integer)
            return new ForthProgramResult(ForthProgramErrorResult.TYPE_MISMATCH, "TIMESPLIT requires the top parameter on the stack to be an integer");

        var offset = DateTimeOffset.FromUnixTimeSeconds((int)si.Value);
        var dt = offset.DateTime;

        stack.Push(new ForthDatum(offset.DateTime.Second));
        stack.Push(new ForthDatum(offset.DateTime.Minute));
        stack.Push(new ForthDatum(offset.DateTime.Hour));
        stack.Push(new ForthDatum(offset.DateTime.Day));
        stack.Push(new ForthDatum(offset.DateTime.Month));
        stack.Push(new ForthDatum(offset.DateTime.Year));
        stack.Push(new ForthDatum(((int)offset.DateTime.DayOfWeek)+1));
        stack.Push(new ForthDatum(offset.DateTime.DayOfYear));

        return default(ForthProgramResult);
    }
}