using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Dbref;
using static ForthDatum;
using static ForthProgramResult;

public static class Trig
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack, Dbref trigger)
    {
        /*
        TRIG ( -- d) 

        Returns the dbref of the original trigger.
        */
        stack.Push(new ForthDatum(trigger, 0));
        return default(ForthProgramResult);
    }
}