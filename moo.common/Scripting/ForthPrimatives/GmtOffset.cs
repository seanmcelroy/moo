using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class GmtOffset
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        GMTOFFSET ( -- i) 

        Returns the machine's offset from Greenwich Mean Time in seconds.
        */
        var ts = DateTime.UtcNow - DateTime.UtcNow;

        parameters.Stack.Push(new ForthDatum(Convert.ToInt32(ts.TotalSeconds)));

        return default(ForthProgramResult);
    }
}