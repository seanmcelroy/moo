using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class LDup
{
    public static ForthProgramResult Execute(ForthPrimativeParameters parameters)
    {
        // LDUP ( {?} -- {?} {?} ) 
        // Duplicates a stackrange on top of the stack.
        foreach (var source in parameters.Stack.ToArray())
            parameters.Stack.Push(source);

        return default(ForthProgramResult);
    }
}