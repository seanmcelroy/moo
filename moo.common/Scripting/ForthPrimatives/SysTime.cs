using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class SysTime
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        /*
        SYSTIME ( -- i ) 

        Returns the number of seconds from Jan 1, 1970 GMT. This is compatible with the system timestamps
        and may be broken down into useful values through 'timesplit'.
        */
        stack.Push(new ForthDatum((int)DateTimeOffset.Now.ToUnixTimeSeconds()));

        return default(ForthProgramResult);
    }
}