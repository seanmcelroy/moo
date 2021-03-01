using System;

public static class Date
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        DATE ( -- i i i) 

        Returns the monthday, month, and year. ie: if it were February 6, 1992, date would return 6 2 1992 as three integers on the stack.
        */
        var now = DateTime.Now;

        parameters.Stack.Push(new ForthDatum(now.Day));
        parameters.Stack.Push(new ForthDatum(now.Month));
        parameters.Stack.Push(new ForthDatum(now.Year));

        return ForthPrimativeResult.SUCCESS;
    }
}