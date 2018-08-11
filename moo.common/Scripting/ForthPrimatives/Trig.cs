using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthPrimativeResult;

public static class Trig
{
    public static ForthPrimativeResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        TRIG ( -- d) 

        Returns the dbref of the original trigger.
        */
        parameters.Stack.Push(new ForthDatum(parameters.Trigger, 0));
        return ForthPrimativeResult.SUCCESS;
    }
}