using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class Version
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        /*
        VERSION ( -- s) 

        Returns the version of this code in a string. "Muck2.2fb5.55", currently.
        */
      
        parameters.Stack.Push(new ForthDatum("Moo0.1"));
        return default(ForthProgramResult);
    }
}