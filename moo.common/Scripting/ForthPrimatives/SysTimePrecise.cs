using System;

public static class SysTimePrecise
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        SYSTIME_PRECISE ( -- f ) 

        Returns the number of seconds from Jan 1, 1970 GMT as a floating point number, with microsecond accuracy.
        */
        var t = DateTime.UtcNow - new DateTime(1970, 1, 1);
        var secondsSinceEpoch = t.TotalMilliseconds / 1000;
        parameters.Stack.Push(new ForthDatum(Convert.ToSingle(secondsSinceEpoch)));

        return ForthPrimativeResult.SUCCESS;
    }
}