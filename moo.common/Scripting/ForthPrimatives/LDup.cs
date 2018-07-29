using System;
using System.Collections.Generic;
using System.Linq;
using static ForthDatum;
using static ForthProgramResult;

public static class LDup
{
    public static ForthProgramResult Execute(Stack<ForthDatum> stack)
    {
        // LDUP ( {?} -- {?} {?} ) 
        // Duplicates a stackrange on top of the stack.
        foreach (var source in stack.ToArray())
            stack.Push(source);

        return default(ForthProgramResult);
    }
}