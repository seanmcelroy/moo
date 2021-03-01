using System;

public static class GmtOffset
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        GMTOFFSET ( -- i) 

        Returns the machine's offset from Greenwich Mean Time in seconds.
        */
        var ts = DateTime.UtcNow - DateTime.UtcNow;

        parameters.Stack.Push(new ForthDatum(Convert.ToInt32(ts.TotalSeconds)));

        return ForthPrimativeResult.SUCCESS;
    }
}